using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessResendPolicy(IAIChatService _aIChatService, 
        ITelegramService _telegramService) : IProcessResendPolicy
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            var msg = "You don't have policy yet, please call /help for support.";
            if (user.Status != DAL.Models.Enums.ProcessStatus.PolicyGenerated)
            {
                await _telegramService.SendMessage(chatId, msg);
                return;
            }

            msg = _aIChatService.PolicyGeneratedMessage();
            using var stream = new MemoryStream(user.Policy!.Content!);
            await _telegramService.SendPolicyAsync(stream, chatId, msg);
        }
    }
}
