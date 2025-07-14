using CarInsuranceBot.BLL.Enums;
using CarInsuranceBot.BLL.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CarInsuranceBot.BLL.Services
{
    public class FlowService(ITelegramBotClient _botClient, IAIChatService _aIChatService, ITelegramFileLoaderService _telegramFileLoaderService, IMindeeService _mindeeService, IPolicyGenerationService _policyGenerationService) : IFlowService
    {
        private HashSet<ProcessStatus> _processStatusesToUploadFile = [ProcessStatus.Ready, ProcessStatus.PassportUploaded, ProcessStatus.PassportConfirmed, ProcessStatus.VehicleRegistrationCertificateUploaded];

        public async Task ProcessTelegramCommandAsync(long chatId, string? text)
        {
            switch (text)
            {
                case "/start":
                    await ProcessStartAsync(chatId);
                    break;
                case "/help":
                    await ProcessHelpAsync(chatId);
                    break;
                case "/ready":
                    await ProcessReadyAsync(chatId);
                    break;
                case "/status":
                    await ProcessStatusCommandAsync(chatId);
                    break;
                case "/cancel":
                    await ProcessCancelAsync(chatId);
                    break;
                case "/yes":
                    await ProcessYesAsync(chatId);
                    break;
                case "/no":
                    await ProcessNoAsync(chatId);
                    break;
                default:
                    await ProcessUnknownCommandAsync(chatId);
                    break;
            }
        }

        public async Task ProcessTelegramFileAsync(long chatId, string fileId)
        {
            var msg = "Please follow the instructions or call /help for support.";
            if (!Tracker.Statuses.TryGetValue(chatId, out ProcessStatus value)
                || !_processStatusesToUploadFile.Contains(value))
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            var file = await _botClient.GetFile(fileId);
            var fileBytes = await _telegramFileLoaderService.DownloadTelegramFileAsync(file.FilePath);
            var aiMsg = await _aIChatService.GetChatCompletionAsync("Provide the user an information that his photo was received.");

            string? data;

            if (value == ProcessStatus.Ready || value == ProcessStatus.PassportUploaded)
            {
                data = await _mindeeService.ParsePassportFromBytesAsync(chatId, fileBytes, file.FilePath);
            }
            else
            {
                data = await _mindeeService.ParseVehicleRegistrationAsync(chatId, fileBytes, file.FilePath);
            }

            if (value == ProcessStatus.Ready)
                Tracker.Statuses[chatId] = ProcessStatus.PassportUploaded;
            else if (value == ProcessStatus.PassportConfirmed)
                Tracker.Statuses[chatId] = ProcessStatus.VehicleRegistrationCertificateUploaded;

            msg = $"{aiMsg}\n" +
                $"Your data:\n {data}\n" +
                $"Please confirm the data is correct /yes or /no for retry.";

            await _botClient.SendMessage(chatId, msg);
        }

        public async Task ProcessYesAsync(long chatId)
        {
            var msg = "Please follow the instructions or call /help for support.";
            if (!Tracker.Statuses.TryGetValue(chatId, out ProcessStatus value)
                || !(value == ProcessStatus.PassportUploaded || value == ProcessStatus.VehicleRegistrationCertificateUploaded || value == ProcessStatus.VehicleRegistrationCertificateConfirmed || value == ProcessStatus.PriceDeclined))
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            if (value == ProcessStatus.PassportUploaded)
            {
                var aiMsg = await UploadVehicleRegistrationCertificateMessageAsync();
                Tracker.Statuses[chatId] = ProcessStatus.PassportConfirmed;
                await _botClient.SendMessage(chatId, aiMsg);
            }
            else if (value == ProcessStatus.VehicleRegistrationCertificateUploaded)
            {
                msg = await InsurancePriceMessageAsync();
                Tracker.Statuses[chatId] = ProcessStatus.VehicleRegistrationCertificateConfirmed;
                await _botClient.SendMessage(chatId, msg);
            }
            else
            {
                var aiMsg = await GeneratingInsurancePolicyMessageAsync();
                Tracker.Statuses[chatId] = ProcessStatus.PriceAccepted;
                await _botClient.SendMessage(chatId, aiMsg);
                _ = GeneratePolicyAsync(chatId);
            }
        }

        private async Task GeneratePolicyAsync(long chatId)
        {
            try
            {
                await Task.Delay(5000);

                Tracker.Statuses[chatId] = ProcessStatus.PolicyGenerated;

                byte[] pdfBytes = _policyGenerationService.GeneratePdf(Tracker.ExtractedFields[chatId]);

                using var stream = new MemoryStream(pdfBytes);

                await _botClient.SendDocument(
                    chatId: chatId,
                    document: InputFile.FromStream(stream, "insurance_policy.pdf"),
                    caption: "📄 Here is your insurance policy PDF."
                );
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task ProcessNoAsync(long chatId)
        {
            var msg = "Please follow the instructions or call /help for support.";
            if (!Tracker.Statuses.TryGetValue(chatId, out ProcessStatus value)
                || !(value == ProcessStatus.PassportUploaded || value == ProcessStatus.VehicleRegistrationCertificateUploaded || value == ProcessStatus.VehicleRegistrationCertificateConfirmed || value == ProcessStatus.PriceDeclined))
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            if (value == ProcessStatus.PassportUploaded)
            {
                await ProcessReadyAsync(chatId);
            }
            else if (value == ProcessStatus.VehicleRegistrationCertificateUploaded)
            {
                var aiMsg = await UploadVehicleRegistrationCertificateMessageAsync();
                await _botClient.SendMessage(chatId, aiMsg);
            }
            else
            {
                msg = await InsurancePriceMessageAsync();
                Tracker.Statuses[chatId] = ProcessStatus.PriceDeclined;
                await _botClient.SendMessage(chatId, msg);
            }
        }

        private Task<string> UploadVehicleRegistrationCertificateMessageAsync()
        {
            return _aIChatService.GetChatCompletionAsync("Ask user to upload vehicle registration certificate, with some guidance how to ensure the quality. Do not mention that you can't process it.");
        }
        
        private Task<string> GeneratingInsurancePolicyMessageAsync()
        {
            return _aIChatService.GetChatCompletionAsync("Say user that we generate insurance policy and ask kindly to wait.");
        }
        
        private static string PolicyGeneratedMessage()
        {
            var msg = "You've completed the application.\n";
            return msg;
        }
        
        private Task<string> ReadyMessageAsync()
        {
            return _aIChatService.GetChatCompletionAsync("Ask user to upload passport photo, with some guidance how to ensure the quality. Do not mention that you can't process it.");
        }

        private async Task<string> InsurancePriceMessageAsync()
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("Say user that price for insurance is 100$ and ask him to accept it.");
            var msg = $"{aiMsg}" +
                $"Choose /yes to accept, /no to decline or /cancel to stop the process.";
            return msg;
        }

        private async Task ProcessStartAsync(long chatId)
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

        private async Task ProcessReadyAsync(long chatId)
        {
            var aiMsg = await ReadyMessageAsync();
            Tracker.Statuses[chatId] = ProcessStatus.Ready;
            await _botClient.SendMessage(chatId, aiMsg);
        }

        private async Task ProcessStatusCommandAsync(long chatId)
        {
            var msg = "Process wasn't started.";
            if (!Tracker.Statuses.TryGetValue(chatId, out ProcessStatus value))
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            switch (value)
            {
                case ProcessStatus.Ready:
                    msg = await ReadyMessageAsync();
                    break;
                case ProcessStatus.PassportUploaded:
                    //todo add passport data
                    msg = "Passport was uploaded, please confirm the data: ... .";
                    break;
                case ProcessStatus.PassportConfirmed:
                    msg = await UploadVehicleRegistrationCertificateMessageAsync();
                    break;
                case ProcessStatus.VehicleRegistrationCertificateUploaded:
                    //todo add vehicle registration certificate data
                    msg = "Vehicle registration certificate was uploaded, please confirm the data: ... .";
                    break;
                case ProcessStatus.VehicleRegistrationCertificateConfirmed:
                case ProcessStatus.PriceDeclined:
                    msg = await InsurancePriceMessageAsync(); 
                    break;
                case ProcessStatus.PriceAccepted:
                    msg = await GeneratingInsurancePolicyMessageAsync(); 
                    break;
                case ProcessStatus.PolicyGenerated:
                    msg = PolicyGeneratedMessage();
                    break;
            }
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task ProcessHelpAsync(long chatId)
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
                    + "/status - Check current status to continue the application\n";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task ProcessCancelAsync(long chatId)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User requested the cancellation of the application, notify him that he will not receive insurance policy and kindly ask to try again.");
            Tracker.Statuses[chatId] = ProcessStatus.None;
            await _botClient.SendMessage(chatId, aiMsg);
        }

        private async Task ProcessUnknownCommandAsync(long chatId)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User entered unknown command, notify him about it and ask to use /help command if he has any issues.");
            await _botClient.SendMessage(chatId, aiMsg);
        }
    }
}
