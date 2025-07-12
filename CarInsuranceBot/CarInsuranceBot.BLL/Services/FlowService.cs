using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.BLL.Services
{
    public class FlowService(ITelegramBotClient _botClient) : IFlowService
    {
        public async Task ProcessTelegramCommand(long chatId, string? text)
        {
            switch (text)
            {
                case "/start":
                    await ProcessStart(chatId, text);
                    break;
                case "/help":
                    await ProcessHelp(chatId, text);
                    break;
                default:
                    await _botClient.SendMessage(chatId, $"Unknown command: {text}");
                    break;
            }
        }

        private async Task ProcessStart(long chatId, string? text)
        {
            var msg = "Hello! 👋\n"
                    + "I’m your Car Insurance Assistant Bot.\n\n"
                    + "Here’s how it works:\n"
                    + "1️⃣ I will guide you step by step to create your insurance policy.\n"
                    + "2️⃣ You’ll need to upload photos of your passport and vehicle registration certificate.\n"
                    + "3️⃣ I will automatically extract the necessary information and show it to you for confirmation.\n"
                    + "4️⃣ After you confirm the details, I’ll generate your insurance policy as a PDF document.\n"
                    + "5️⃣ You will receive the policy file directly here in this chat.\n\n"
                    + "At any time, you can type /help for assistance.\n\n"
                    + "Let’s get started when you are ready!";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task ProcessHelp(long chatId, string? text)
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
    }
}
