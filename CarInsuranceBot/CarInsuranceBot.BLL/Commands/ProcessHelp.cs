using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessHelp(ITelegramBotClient _botClient) : IProcessHelp
    {
        public async Task ProcessAsync(long chatId)
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
                    + "/cancel - Cancel current process\n"
                    + "/status - Check current status to continue the application\n"
                    + "/resendpolicy - Resend your policy\n";
            await _botClient.SendMessage(chatId, msg);
        }
    }
}
