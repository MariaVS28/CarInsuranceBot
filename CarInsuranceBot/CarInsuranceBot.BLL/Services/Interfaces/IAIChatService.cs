namespace CarInsuranceBot.BLL.Services
{
    public interface IAIChatService
    {
        Task<string> GetChatCompletionAsync(string userMessage);
    }
}
