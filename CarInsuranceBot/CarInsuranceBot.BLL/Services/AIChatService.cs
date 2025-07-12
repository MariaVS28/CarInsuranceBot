using System.Text.Json;
using System.Text;

namespace CarInsuranceBot.BLL.Services
{
    public class AIChatService (HttpClient _httpClient) : IAIChatService
    {
        public async Task<string> GetChatCompletionAsync(string userMessage)
        {
            try
            {
                var geminiToken = Environment.GetEnvironmentVariable("GEMINI_TOKEN");
                if (geminiToken == null) throw new Exception("Missing environment variable.");

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
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro-latest:generateContent?key={geminiToken}"
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
                throw ex;
            }
        }
    }
}
