using System.Text.Json;
using System.Text;
using CarInsuranceBot.DAL.Repositories;
using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Services
{
    public class AIChatService (HttpClient _httpClient, IErrorRepository _errorRepository) : IAIChatService
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
                    Date = DateTime.UtcNow
                };

                await _errorRepository.AddErrorAsync(error);

                throw ex;
            }
        }
    }
}
