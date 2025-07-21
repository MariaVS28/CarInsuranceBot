using CarInsuranceBot.BLL.Services;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessUnknown(ITelegramBotClient _botClient, IAIChatService _aIChatService) : IProcessUnknown
    {
        public async Task ProcessAsync(long chatId)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User entered unknown command, notify him about it and ask to use /help command if he has any issues.");
            await _botClient.SendMessage(chatId, aiMsg);
        }
    }
}
