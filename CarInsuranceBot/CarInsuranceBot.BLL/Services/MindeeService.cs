using CarInsuranceBot.DAL.Models;
using Mindee;
using Mindee.Input;
using Mindee.Product.Passport;

namespace CarInsuranceBot.BLL.Services
{
    public class MindeeService(MindeeClient _mindeeClient, HttpClient _httpClient) : IMindeeService
    {
        public async Task<string> ParsePassportFromBytesAsync(long chatId, byte[] fileBytes, string filePath)
        {
            try
            {
                using var stream = new MemoryStream(fileBytes);

                var inputSource = new LocalInputSource(stream, filePath);

                var prediction = await _mindeeClient
                    .ParseAsync<PassportV1>(inputSource);

                var passportData = prediction.Document.Inference.Prediction;

                Tracker.ExtractedFields[chatId] = Tracker.ExtractedFields.GetValueOrDefault(chatId, new ExtractedFields());
                Tracker.ExtractedFields[chatId].PassportNumber = passportData.IdNumber?.Value;
                Tracker.ExtractedFields[chatId].Surname = passportData.Surname?.Value;
                Tracker.ExtractedFields[chatId].GivenNames = string.Join(" ", passportData.GivenNames);
                Tracker.ExtractedFields[chatId].BirthDate = passportData.BirthDate?.Value;
                Tracker.ExtractedFields[chatId].ExpiryDate = passportData.ExpiryDate?.Value;


                return $"Passport Number: {passportData.IdNumber?.Value}\n" +
                       $"Surname: {passportData.Surname?.Value}\n" +
                       $"Given Names: {string.Join(" ", passportData.GivenNames)}\n" +
                       $"Date of Birth: {passportData.BirthDate?.Value}\n" +
                       $"Expiry Date: {passportData.ExpiryDate?.Value}";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> ParseVehicleRegistrationAsync(long chatId, byte[] fileBytes, string filePath)
        {
            try
            {
                // Send Vehicle Registration to mindee and parse data. But I don't have an access to CarteGriseV1 API, it's paid vertion.

                //using var stream = new MemoryStream(fileBytes);

                //var inputSource = new LocalInputSource(stream, filePath);

                //var prediction = await _mindeeClient
                //    .ParseAsync<CarteGriseV1>(inputSource);

                //var data = prediction.Document.Inference.Prediction;

                //return $"The vehicle's license plate number: {data.A?.Value}\n" +
                //   $"The vehicle's first release date: {data.B?.Value}\n" +
                //   $"The vehicle owner's full name: {data.C1?.Value}\n" +
                //   $"The vehicle's brand: {data.D1?.Value}\n" +
                //   $"The Vehicle Identification Number (VIN): {data.E?.Value}";

                Tracker.ExtractedFields[chatId] = Tracker.ExtractedFields.GetValueOrDefault(chatId, new ExtractedFields());
                Tracker.ExtractedFields[chatId].VehiclesLicensePlateNumber = "AB-123-CD";
                Tracker.ExtractedFields[chatId].VehicleFirstReleaseDate = "1998-01-05";
                Tracker.ExtractedFields[chatId].VehicleOwnersFullName = "JERSEY SPECIMEN ANGELA ZOE";
                Tracker.ExtractedFields[chatId].VehiclesBrand = "FORD";
                Tracker.ExtractedFields[chatId].VIN = "VFS1V2009AS1V2009";

                return $"The vehicle's license plate number: AB-123-CD\n" +
                   $"The vehicle's first release date: 1998-01-05\n" +
                   $"The vehicle owner's full name: JERSEY SPECIMEN ANGELA ZOE\n" +
                   $"The vehicle's brand: FORD\n" +
                   $"The Vehicle Identification Number (VIN): VFS1V2009AS1V2009";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        // Method for parsing all types of documents. It's also paid verion.
        public async Task<string> ParseGenericOcrAsync(byte[] fileBytes)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(fileBytes), "document", "document.jpg");

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "products/mindee/document/v1/predict"
                )
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
