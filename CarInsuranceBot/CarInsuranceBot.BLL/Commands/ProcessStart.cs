using CarInsuranceBot.BLL.Services;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessStart(IAIChatService _aIChatService, ITelegramService _telegramService) : IProcessStart
    {
        public async Task ProcessAsync(long chatId)
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
            await _telegramService.SendMessage(chatId, msg);
        }
    }
}
