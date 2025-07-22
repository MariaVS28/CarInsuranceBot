using CarInsuranceBot.BLL.Services;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessUnknown(ITelegramService _telegramService, IAIChatService _aIChatService) : IProcessUnknown
    {
        public async Task ProcessAsync(long chatId)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User entered unknown command, notify him about it and ask to use /help command if he has any issues.");
            await _telegramService.SendMessage(chatId, aiMsg);
        }
    }
}
