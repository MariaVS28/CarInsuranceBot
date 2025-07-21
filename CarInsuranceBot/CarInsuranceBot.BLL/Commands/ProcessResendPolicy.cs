using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessResendPolicy(ITelegramBotClient _botClient, IAIChatService _aIChatService, 
        ITelegramService _telegramService) : IProcessResendPolicy
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            var msg = "You don't have policy yet, please call /help for support.";
            if (user.Status != DAL.Models.Enums.ProcessStatus.PolicyGenerated)
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            msg = _aIChatService.PolicyGeneratedMessage();
            using var stream = new MemoryStream(user.Policy!.Content!);
            await _telegramService.SendPolicyAsync(stream, chatId, msg);
        }
    }
}
