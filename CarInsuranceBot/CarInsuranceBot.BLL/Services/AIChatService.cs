using System.Text.Json;
using System.Text;
using CarInsuranceBot.DAL.Repositories;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.BLL.Helpers;

namespace CarInsuranceBot.BLL.Services
{
    public class AIChatService (HttpClient _httpClient, IErrorRepository _errorRepository, IDateTimeHelper _dateTimeHelper) : IAIChatService
    {
        public async Task<string> GetChatCompletionAsync(string userMessage)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
            {
                new {
                    role = "user",
                    parts = new[]
                    {
                        new { text = userMessage }
                    }
                }
            }
                };

                var requestJson = JsonSerializer.Serialize(requestBody);

                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $""
                )
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseJson = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseJson);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text;
            }
            catch (Exception ex)
            {
                var error = new Error
                {
                    StackTrace = ex.StackTrace,
                    Message = ex.Message,
                    FaildStep = FaildStep.ChatCompletion,
                    Date = _dateTimeHelper.UtcNow()
                };

                await _errorRepository.AddErrorAsync(error);

                throw ex;
            }
        }
        public Task<string> UploadVehicleRegistrationCertificateMessageAsync()
        {
            return GetChatCompletionAsync("Ask user to upload vehicle registration certificate, with some guidance how to ensure the quality. Do not mention that you can't process it.");
        }

        public Task<string> ApprovalInsurancePolicyMessageAsync()
        {
            return GetChatCompletionAsync("Say user that admin looks thowgh and answer on application, ask kindly to wait.");
        }

        public string PolicyGeneratedMessage()
        {
            var msg = "You've completed the application.\n";
            return msg;
        }

        public Task<string> ReadyMessageAsync()
        {
            return GetChatCompletionAsync("Ask user to upload passport photo, with some guidance how to ensure the quality. Do not mention that you can't process it.");
        }

        public async Task<string> InsurancePriceMessageAsync()
        {
            var aiMsg = await GetChatCompletionAsync("Say user that price for insurance is 100$ and ask him to accept it.");
            var msg = $"{aiMsg}" +
                $"Choose /yes to accept, /no to decline or /cancel to stop the process.";
            return msg;
        }
    }
}
