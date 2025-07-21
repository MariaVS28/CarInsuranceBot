namespace CarInsuranceBot.BLL.Services
{
    public interface ITelegramService
    {
        Task SendPolicyAsync(MemoryStream stream, long chatId, string? msg = null);
    }
}
