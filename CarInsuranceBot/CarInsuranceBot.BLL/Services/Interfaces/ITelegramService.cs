using Telegram.Bot.Types;

namespace CarInsuranceBot.BLL.Services
{
    public interface ITelegramService
    {
        Task SendPolicyAsync(MemoryStream stream, long chatId, string? msg = null);
        Task<Message> SendMessage(long chatId, string? message);
    }
}
