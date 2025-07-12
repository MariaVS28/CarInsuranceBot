using System.Net.Http;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Services
{
    public class FlowService(ITelegramBotClient _botClient, IAIChatService _aIChatService, ITelegramFileLoaderService _telegramFileLoaderService) : IFlowService
    {
        public async Task ProcessTelegramCommand(long chatId, string? text)
        {
            switch (text)
            {
                case "/start":
                    await ProcessStart(chatId);
                    break;
                case "/help":
                    await ProcessHelp(chatId);
                    break;
                case "/ready":
                    await ProcessReady(chatId);
                    break;
                default:
                    await ProcessUnknownCommand(chatId);
                    break;
            }
        }

        public async Task ProcessTelegramFile(long chatId, string fileId)
        {
            var file = await _botClient.GetFile(fileId);
            var fileBytes = await _telegramFileLoaderService.DownloadTelegramFileAsync(file.FilePath);
            var aiMsg = await _aIChatService.GetChatCompletionAsync("Provide the user an information that his photo was received.");
            await _botClient.SendMessage(chatId, aiMsg);
        }

        private async Task ProcessStart(long chatId)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("Greet the user of car insurance service.");
            var msg = "Hello! 👋\n"
                    + $"{aiMsg}\n"
                    + "Here’s how it works:\n"
                    + "1️⃣ I will guide you step by step to create your insurance policy.\n"
                    + "2️⃣ You’ll need to upload photos of your passport and vehicle registration certificate.\n"
                    + "3️⃣ I will automatically extract the necessary information and show it to you for confirmation.\n"
                    + "4️⃣ After you confirm the details, I’ll generate your insurance policy as a PDF document.\n"
                    + "5️⃣ You will receive the policy file directly here in this chat.\n\n"
                    + "At any time, you can type /help for assistance.\n\n"
                    + "Let’s get started when you are ready!\n"
                    + "Click /ready to begin process";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task ProcessReady(long chatId)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("Ask user to upload passport photo, with some guidance how to ensure the quality. Do not mention that you can't process it.");
            await _botClient.SendMessage(chatId, aiMsg);
        }

        private async Task ProcessHelp(long chatId)
        {
            var msg = "Here’s how it works:\n"
                    + "1️⃣ I will guide you step by step to create your insurance policy.\n"
                    + "2️⃣ You’ll need to upload photos of your passport and vehicle registration certificate.\n"
                    + "3️⃣ I will automatically extract the necessary information and show it to you for confirmation.\n"
                    + "4️⃣ After you confirm the details, I’ll generate your insurance policy as a PDF document.\n"
                    + "5️⃣ You will receive the policy file directly here in this chat.\n\n"
                    + "List of available commands:\n"
                    + "/start - Start working with the bot\n"
                    + "/help - Show instructions\n"
                    + "/cancel - Cancel current process\n";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task ProcessUnknownCommand(long chatId)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User entered unknown command, notify him about it and ask to use /help command if he has any issues.");
            await _botClient.SendMessage(chatId, aiMsg);
        }
    }
}
