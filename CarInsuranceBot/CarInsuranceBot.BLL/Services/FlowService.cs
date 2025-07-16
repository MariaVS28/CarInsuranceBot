using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.BLL.Services.Interfaces;
using Telegram.Bot;
using CarInsuranceBot.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CarInsuranceBot.BLL.Services
{
    public class FlowService(ITelegramBotClient _botClient, IAIChatService _aIChatService, 
        ITelegramFileLoaderService _telegramFileLoaderService, IMindeeService _mindeeService, 
        IUserRepository _userRepository, IServiceScopeFactory _scopeFactory) : IFlowService
    {
        private readonly HashSet<ProcessStatus> _processStatusesToUploadFile = [ProcessStatus.Ready, ProcessStatus.PassportUploaded, ProcessStatus.PassportConfirmed, ProcessStatus.VehicleRegistrationCertificateUploaded];

        public async Task ProcessTelegramCommandAsync(long chatId, string? text)
        {
            var user = await _userRepository.GetUserAsync(chatId);
            if (user == null 
                && (text != "/start" && text != "/help" && text != "/ready" && text != "/status"))
            {
                await ProcessUnknownCommandAsync(chatId);
                return;
            }

            switch (text)
            {
                case "/start":
                    await ProcessStartAsync(chatId);
                    break;
                case "/help":
                    await ProcessHelpAsync(chatId);
                    break;
                case "/ready":
                    await ProcessReadyAsync(chatId, user);
                    break;
                case "/status":
                    await ProcessStatusCommandAsync(chatId, user);
                    break;
                case "/cancel":
                    await ProcessCancelAsync(chatId, user!);
                    break;
                case "/yes":
                    await ProcessYesAsync(chatId, user!);
                    break;
                case "/no":
                    await ProcessNoAsync(chatId, user!);
                    break;
                default:
                    await ProcessUnknownCommandAsync(chatId);
                    break;
            }
        }

        public async Task ProcessTelegramFileAsync(long chatId, string fileId)
        {
            var msg = "Please follow the instructions or call /help for support.";
            var user = await _userRepository.GetUserAsync(chatId);
            if (user == null
                || !_processStatusesToUploadFile.Contains(user.Status))
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            var file = await _botClient.GetFile(fileId);
            var fileBytes = await _telegramFileLoaderService.DownloadTelegramFileAsync(file.FilePath);
            var aiMsg = await _aIChatService.GetChatCompletionAsync("Provide the user an information that his photo was received.");

            string? data;

            if (user.Status == ProcessStatus.Ready || user.Status == ProcessStatus.PassportUploaded)
            {
                data = await _mindeeService.ParsePassportFromBytesAsync(chatId, fileBytes, file.FilePath, user);
            }
            else
            {
                data = await _mindeeService.ParseVehicleRegistrationAsync(chatId, fileBytes, file.FilePath, user);
            }

            if (user.Status == ProcessStatus.Ready)
            {
                user.Status = ProcessStatus.PassportUploaded;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();
            }  
            else if (user.Status == ProcessStatus.PassportConfirmed)
            {
                user.Status = ProcessStatus.VehicleRegistrationCertificateUploaded;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();
            }

            msg = $"{aiMsg}\n" +
                $"Your data:\n {data}\n" +
                $"Please confirm the data is correct /yes or /no for retry.";

            await _botClient.SendMessage(chatId, msg);
        }

        public async Task ProcessYesAsync(long chatId, User user)
        {
            var msg = "Please follow the instructions or call /help for support.";
            if (!(user.Status == ProcessStatus.PassportUploaded || user.Status == ProcessStatus.VehicleRegistrationCertificateUploaded
                || user.Status == ProcessStatus.VehicleRegistrationCertificateConfirmed || user.Status == ProcessStatus.PriceDeclined))
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            if (user.Status == ProcessStatus.PassportUploaded)
            {
                var aiMsg = await UploadVehicleRegistrationCertificateMessageAsync();
                user.Status = ProcessStatus.PassportConfirmed;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();
                await _botClient.SendMessage(chatId, aiMsg);
            }
            else if (user.Status == ProcessStatus.VehicleRegistrationCertificateUploaded)
            {
                msg = await InsurancePriceMessageAsync();
                user.Status = ProcessStatus.VehicleRegistrationCertificateConfirmed;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();
                await _botClient.SendMessage(chatId, msg);
            }
            else
            {
                var aiMsg = await GeneratingInsurancePolicyMessageAsync();
                user.Status = ProcessStatus.PriceAccepted;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();
                await _botClient.SendMessage(chatId, aiMsg);
                _ = GeneratePolicyAsync(chatId);
            }
        }

        private async Task GeneratePolicyAsync(long chatId)
        {
            using var scope = _scopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var policyGenerationService = scope.ServiceProvider.GetRequiredService<IPolicyGenerationService>();

            User? user = null;
            try
            {
                user = await userRepository.GetUserAsync(chatId);
                user!.Policy = new Policy
                {
                    User = user,
                    Status = PolicyProcessStatus.InProgress
                };
                await userRepository.SaveChangesAsync();

                byte[] pdfBytes = await policyGenerationService.GeneratePdfAsync(user.ExtractedFields!);

                using var stream = new MemoryStream(pdfBytes);

                user.Status = ProcessStatus.PolicyGenerated;
                user.Policy.Content = pdfBytes;
                user.Policy.Status = PolicyProcessStatus.Completed;
                user.Policy.Title = "insurance_policy.pdf";
                await userRepository.SaveChangesAsync();

                await _botClient.SendDocument(
                    chatId: chatId,
                    document: Telegram.Bot.Types.InputFile.FromStream(stream, "insurance_policy.pdf"),
                    caption: "📄 Here is your insurance policy PDF."
                );
            }
            catch(Exception ex)
            {
                if (user.Policy != null)
                {
                    user.Policy.Status = PolicyProcessStatus.Failed;
                    await userRepository.SaveChangesAsync();
                }
                
                throw ex;
            }
        }

        public async Task ProcessNoAsync(long chatId, User user)
        {
            var msg = "Please follow the instructions or call /help for support.";
            if (!(user.Status == ProcessStatus.PassportUploaded || user.Status == ProcessStatus.VehicleRegistrationCertificateUploaded 
                || user.Status == ProcessStatus.VehicleRegistrationCertificateConfirmed || user.Status == ProcessStatus.PriceDeclined))
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            if (user.Status == ProcessStatus.PassportUploaded)
            {
                await ProcessReadyAsync(chatId, user);
            }
            else if (user.Status == ProcessStatus.VehicleRegistrationCertificateUploaded)
            {
                var aiMsg = await UploadVehicleRegistrationCertificateMessageAsync();
                await _botClient.SendMessage(chatId, aiMsg);
            }
            else
            {
                msg = await InsurancePriceMessageAsync();
                user.Status = ProcessStatus.PriceDeclined;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();
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

        private async Task ProcessReadyAsync(long chatId, User? user)
        {
            var aiMsg = await ReadyMessageAsync();
            if (user  == null)
            {
                user = new User
                {
                    UserId = chatId,
                    Status = ProcessStatus.Ready,
                    LastUpdated = DateTime.UtcNow,
                };
                await _userRepository.AddUserAsync(user);
            }
            else
            {
                user.Status = ProcessStatus.Ready;
                await _userRepository.SaveChangesAsync();
            }
            
            await _botClient.SendMessage(chatId, aiMsg);
        }

        private async Task ProcessStatusCommandAsync(long chatId, User? user)
        {
            var msg = "Process wasn't started.";
            if (user == null)
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            switch (user.Status)
            {
                case ProcessStatus.Ready:
                    msg = await ReadyMessageAsync();
                    break;
                case ProcessStatus.PassportUploaded:
                    msg = "Passport was uploaded, please confirm the data:\n" +
                       $"Passport Number: {user.ExtractedFields?.PassportNumber}\n" +
                       $"Surname: {user.ExtractedFields?.Surname}\n" +
                       $"Given Names: {string.Join(" ", user.ExtractedFields?.GivenNames)}\n" +
                       $"Date of Birth: {user.ExtractedFields?.BirthDate}\n" +
                       $"Expiry Date: {user.ExtractedFields?.ExpiryDate}";
                    break;
                case ProcessStatus.PassportConfirmed:
                    msg = await UploadVehicleRegistrationCertificateMessageAsync();
                    break;
                case ProcessStatus.VehicleRegistrationCertificateUploaded:
                    msg = "Vehicle registration certificate was uploaded, please confirm the data:\n" +
                         $"Vehicle Owner's Full Name: {user.ExtractedFields?.VehicleOwnersFullName}\n" +
                         $"Vehicle's Registration Date: {user.ExtractedFields?.VehiclesRegistrationDate}\n" +
                         $"Vehicle Identification Number: {user.ExtractedFields?.VehicleIdentificationNumber}\n" +
                         $"Vehicle Make: {user.ExtractedFields?.VehicleMake}\n" +
                         $"Vehicle Model: {user.ExtractedFields?.VehicleModel}";
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
                    using (var stream = new MemoryStream(user.Policy!.Content!))
                    {
                        await _botClient.SendDocument(
                        chatId: chatId,
                        document: Telegram.Bot.Types.InputFile.FromStream(stream, "insurance_policy.pdf"),
                        caption: $"{msg}📄 Here is your insurance policy PDF.");
                    }

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

        private async Task ProcessCancelAsync(long chatId, User user)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User requested the cancellation of the application, notify him that he will not receive insurance policy and kindly ask to try again.");
            user.Status = ProcessStatus.None;
            await _userRepository.SaveChangesAsync();
            await _botClient.SendMessage(chatId, aiMsg);
        }

        private async Task ProcessUnknownCommandAsync(long chatId)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User entered unknown command, notify him about it and ask to use /help command if he has any issues.");
            await _botClient.SendMessage(chatId, aiMsg);
        }
    }
}
