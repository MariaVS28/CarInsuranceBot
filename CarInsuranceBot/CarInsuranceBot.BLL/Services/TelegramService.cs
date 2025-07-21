using CarInsuranceBot.BLL.Services.Interfaces;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Services
{
    public class TelegramService(ITelegramBotClient _botClient) : ITelegramService
    {
        public async Task SendPolicyAsync(MemoryStream stream, long chatId, string? msg = null)
        {
            await _botClient.SendDocument(
             chatId: chatId,
             document: Telegram.Bot.Types.InputFile.FromStream(stream, "insurance_policy.pdf"),
             caption: $"{msg}📄 Here is your insurance policy PDF.");
        }
    }
}
