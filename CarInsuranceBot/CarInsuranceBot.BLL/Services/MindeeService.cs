using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using Mindee;
using Mindee.Input;
using Mindee.Product.Passport;
using Newtonsoft.Json.Linq;

namespace CarInsuranceBot.BLL.Services
{
    public class MindeeService(MindeeClient _mindeeClient, HttpClient _httpClient, IUserRepository _userRepository, 
        IErrorRepository _errorRepository) : IMindeeService
    {
        public async Task<string> ParsePassportFromBytesAsync(long chatId, byte[] fileBytes, string filePath, User user)
        {
            try
            {
                using var stream = new MemoryStream(fileBytes);

                var inputSource = new LocalInputSource(stream, filePath);

                var prediction = await _mindeeClient
                    .ParseAsync<PassportV1>(inputSource);

                var passportData = prediction.Document.Inference.Prediction;

                user.ExtractedFields ??= new ExtractedFields();
                user.ExtractedFields.PassportNumber = passportData.IdNumber?.Value;
                user.ExtractedFields.Surname = passportData.Surname?.Value;
                user.ExtractedFields.GivenNames = string.Join(" ", passportData.GivenNames);
                user.ExtractedFields.BirthDate = passportData.BirthDate?.Value;
                user.ExtractedFields.ExpiryDate = passportData.ExpiryDate?.Value;

                user.Documents.Add(new Document
                {
                    User = user,
                    Content = fileBytes,
                    Title = filePath
                });

                await _userRepository.SaveChangesAsync();

                return $"Passport Number: {passportData.IdNumber?.Value}\n" +
                       $"Surname: {passportData.Surname?.Value}\n" +
                       $"Given Names: {string.Join(" ", passportData.GivenNames)}\n" +
                       $"Date of Birth: {passportData.BirthDate?.Value}\n" +
                       $"Expiry Date: {passportData.ExpiryDate?.Value}";

                //return $"Passport Number: 999228775\n" +
                //       $"Surname: JERSEY SPECIMEN\n" +
                //       $"Given Names: ANGELA ZOE\n" +
                //       $"Date of Birth: 1995-01-01\n" +
                //       $"Expiry Date: 2029-11-27";
            }
            catch (Exception ex)
            {
                var error = new Error
                {
                    StackTrace = ex.StackTrace,
                    Message = ex.Message,
                    Date = DateTime.UtcNow
                };

                await _errorRepository.AddErrorAsync(error);
                throw ex;
            }
        }

        public async Task<string> ParseVehicleRegistrationAsync(long chatId, byte[] fileBytes, string filePath, User user)
        {
            var mindeeVrcKey = Environment.GetEnvironmentVariable("MINDEE_VRC_KEY");
            var mindeeVrcAccount = Environment.GetEnvironmentVariable("MINDEE_VRC_ACCOUNT");
            if (mindeeVrcKey == null || mindeeVrcAccount == null) throw new Exception("Missing environment variable.");

            user.Documents.Add(new Document
            {
                User = user,
                Content = fileBytes,
                Title = filePath
            });

            await _userRepository.SaveChangesAsync();

            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(fileBytes), "document", filePath);

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"v1/products/{mindeeVrcAccount}/vehicle_registration_certificate/v1/predict_async"
                )
                {
                    Content = content
                };
                request.Headers.Add("Authorization", $"Token {mindeeVrcKey}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadAsStringAsync();

                var jObject = JObject.Parse(result);
                var queueId = (string?)jObject.SelectToken("job.id");
                if (queueId == null)
                    throw new Exception("Failed to parse vehicle registration certificate");

                return await PollVehicleRegistration(chatId, queueId, mindeeVrcKey, mindeeVrcAccount, user);
            }
            catch (Exception ex)
            {
                var error = new Error
                {
                    StackTrace = ex.StackTrace,
                    Message = ex.Message,
                    Date = DateTime.UtcNow
                };

                await _errorRepository.AddErrorAsync(error);
                throw ex;
            }
        }

        private async Task<string> PollVehicleRegistration(long chatId, string queueId, string mindeeVrcKey, string mindeeVrcAccount, User user)
        {
            try
            {
                const int maxRetries = 10;
                var tries = 0;
                while (tries < maxRetries)
                {
                    await Task.Delay(2000);

                    var pollRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"v1/products/{mindeeVrcAccount}/vehicle_registration_certificate/v1/documents/queue/{queueId}");

                    pollRequest.Headers.Add("Authorization", $"Token {mindeeVrcKey}");
                    var pollResponse = await _httpClient.SendAsync(pollRequest);

                    if ((int)pollResponse.StatusCode >= 300 && (int)pollResponse.StatusCode < 400)
                    {
                        var redirectUri = pollResponse.Headers.Location;

                        var redirectRequest = new HttpRequestMessage(HttpMethod.Get, redirectUri);
                        redirectRequest.Headers.Add("Authorization", $"Token {mindeeVrcKey}");

                        pollResponse = await _httpClient.SendAsync(redirectRequest);
                    }

                    pollResponse.EnsureSuccessStatusCode();

                    var pollResult = await pollResponse.Content.ReadAsStringAsync();
                    var polljObject = JObject.Parse(pollResult);
                    var status = (string?)polljObject.SelectToken("job.status");
                    if (status == null)
                        throw new Exception("Failed to parse vehicle registration certificate");

                    if (status == "completed" || status == "done")
                    {
                        var data = polljObject.SelectToken("document.inference.prediction");

                        if (data == null)
                            throw new Exception("Failed to parse vehicle registration certificate");

                        user.ExtractedFields ??= new ExtractedFields();
                        user.ExtractedFields.VehicleOwnersFullName = data.SelectToken("owner_name.value")?.ToString();
                        user.ExtractedFields.VehiclesRegistrationDate = data["registration_date"]?.Value<DateTime>("value");
                        user.ExtractedFields.VehicleIdentificationNumber = data.SelectToken("vehicle_identification_number.value")?.ToString();
                        user.ExtractedFields.VehicleMake = data.SelectToken("vehicle_make.value")?.ToString();
                        user.ExtractedFields.VehicleModel = data.SelectToken("vehicle_model.value")?.ToString();

                        await _userRepository.SaveChangesAsync();

                        return $"Vehicle Owner's Full Name: {data.SelectToken("owner_name.value")?.ToString()}\n" +
                               $"Vehicle's Registration Date: {data["registration_date"]?.Value<DateTime>("value")}\n" +
                               $"Vehicle Identification Number: {data.SelectToken("vehicle_identification_number.value")?.ToString()}\n" +
                               $"Vehicle Make: {data.SelectToken("vehicle_make.value")?.ToString()}\n" +
                               $"Vehicle Model: {data.SelectToken("vehicle_model.value")?.ToString()}";
                    }
                    else if (status == "failed")
                    {
                        throw new Exception("Failed to parse vehicle registration certificate");
                    }

                    tries++;
                }

                throw new Exception("Failed to parse vehicle registration certificate");
            }
            catch (Exception ex) 
            {
                var error = new Error
                {
                    StackTrace = ex.StackTrace,
                    Message = ex.Message,
                    Date = DateTime.UtcNow
                };

                await _errorRepository.AddErrorAsync(error);
                throw ex;
            }
        }
    }
}
