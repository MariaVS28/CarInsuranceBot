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
        IUserRepository _userRepository, IAuditLogRepository _auditLogRepository, 
        IErrorRepository _errorRepository, IServiceScopeFactory _scopeFactory,
        IPolicyRepository _policyRepository, IExtractedFieldsRepository _extractedFieldsRepository) : IFlowService
    {
        private readonly HashSet<ProcessStatus> _processStatusesToUploadFile = [ProcessStatus.Ready, ProcessStatus.PassportUploaded, ProcessStatus.PassportConfirmed, ProcessStatus.VehicleRegistrationCertificateUploaded];

        public async Task ProcessTelegramCommandAsync(long chatId, string? text, Telegram.Bot.Types.User telegramUser)
        {
            var user = await _userRepository.GetUserAsync(chatId);
            if (user == null 
                && (text != "/start" && text != "/help" && text != "/ready" && text != "/status"))
            {
                await ProcessUnknownCommandAsync(chatId);
                return;
            }

            long targetId = 0;

            if (text.StartsWith($"/giveadmin"))
            {
                targetId = GetTargetId(text);
                text = "/giveadmin";
            }
            else if (text.StartsWith($"/revokeadmin"))
            {
                targetId = GetTargetId(text);
                text = "/revokeadmin";
            }
            else if (text.StartsWith($"/approvepolicy"))
            {
                targetId = GetTargetId(text);
                text = "/approvepolicy";
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
                    await ProcessReadyAsync(chatId, user, telegramUser);
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
                case "/resendpolicy":
                    await ProcessResendPolicyAsync(chatId, user!);
                    break;
                case "/giveadmin":
                    await GiveAdminCommandAsync(chatId, user!, targetId);
                    break;
                case "/revokeadmin":
                    await RevokeAdminCommandAsync(chatId, user!, targetId);
                    break;
                case "/policiessummary":
                    await PoliciesSummaryCommandAsync(chatId, user!);
                    break;
                case "/faildpolicieslogs":
                    await FaildPoliciesLogsCommandAsync(chatId, user!);
                    break;
                case "/mockdocumentdata":
                    await MockDocumentDataCommandAsync(chatId, user!);
                    break;
                case "/unmockdocumentdata":
                    await UnMockDocumentDataCommandAsync(chatId, user!);
                    break;
                case "/getpendingpolicies":
                    await GetPendingPoliciesCommandAsync(chatId, user!);
                    break;
                case "/approvepolicy":
                    await ApprovePolicyCommandAsync(chatId, user!, targetId);
                    break;
                default:
                    await ProcessUnknownCommandAsync(chatId);
                    break;
            }
        }

        private long GetTargetId(string text)
        {
            long targetId;

            int lastSpaceIndex = text.LastIndexOf(' ');

            var result = lastSpaceIndex >= 0
                ? text.Substring(lastSpaceIndex + 1)
                : text;

            long.TryParse(result, out targetId);

            return targetId;
        }

        public async Task ProcessTelegramFileAsync(long chatId, string fileId)
        {
            try
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
                    user.FileUploadAttempts ??= new();
                    user.FileUploadAttempts!.PassportAttemps++;
                    await _userRepository.SaveChangesAsync();

                    data = await _mindeeService.ParsePassportFromBytesAsync(chatId, fileBytes, file.FilePath, user);
                }
                else
                {
                    user.FileUploadAttempts ??= new();
                    user.FileUploadAttempts!.VRCAttemps++;
                    await _userRepository.SaveChangesAsync();

                    data = await _mindeeService.ParseVehicleRegistrationAsync(chatId, fileBytes, file.FilePath, user);
                }

                if (user.Status == ProcessStatus.Ready)
                {
                    user.Status = ProcessStatus.PassportUploaded;
                    user.LastUpdated = DateTime.UtcNow;
                    await _userRepository.SaveChangesAsync();

                    var auditLog = new AuditLog
                    {
                        Message = $"The User {user.UserId} uploaded passport",
                        Date = DateTime.UtcNow
                    };
                    await _auditLogRepository.AddAuditLogAsync(auditLog);
                }
                else if (user.Status == ProcessStatus.PassportConfirmed)
                {
                    user.Status = ProcessStatus.VehicleRegistrationCertificateUploaded;
                    user.LastUpdated = DateTime.UtcNow;
                    await _userRepository.SaveChangesAsync();

                    var auditLog = new AuditLog
                    {
                        Message = $"The User {user.UserId} uploaded Vehicle Registration Certificate",
                        Date = DateTime.UtcNow
                    };
                    await _auditLogRepository.AddAuditLogAsync(auditLog);
                }

                msg = $"{aiMsg}\n" +
                    $"Your data:\n {data}\n" +
                    $"Please confirm the data is correct /yes or /no for retry.";

                await _botClient.SendMessage(chatId, msg);
            }
            catch (Exception ex)
            {
                var error = new Error
                {
                    StackTrace = ex.StackTrace,
                    Message = $"User {chatId}" + " " + ex.Message,
                    FaildStep = FaildStep.ProcessFile,
                    Date = DateTime.UtcNow
                };

                await _errorRepository.AddErrorAsync(error);
                throw ex;
            }
        }

        private async Task ProcessYesAsync(long chatId, User user)
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

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} confirmed passpord data",
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _botClient.SendMessage(chatId, aiMsg);
            }
            else if (user.Status == ProcessStatus.VehicleRegistrationCertificateUploaded)
            {
                msg = await InsurancePriceMessageAsync();
                user.Status = ProcessStatus.VehicleRegistrationCertificateConfirmed;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} confirmed Vehicle Registration Certificate",
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _botClient.SendMessage(chatId, msg);
            }
            else
            {
                var aiMsg = await ApprovalInsurancePolicyMessageAsync();
                user.Status = ProcessStatus.PriceAccepted;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                user!.Policy = new Policy
                {
                    User = user,
                    Status = PolicyProcessStatus.InProgress
                };
                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} accepted insurance price",
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _botClient.SendMessage(chatId, aiMsg);
            }
        }

        private async Task GeneratePolicyAsync(long targetId)
        {
            using var scope = _scopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var policyGenerationService = scope.ServiceProvider.GetRequiredService<IPolicyGenerationService>();
            var auditLogRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
            var errorRepository = scope.ServiceProvider.GetRequiredService<IErrorRepository>();

            User? user = null;
            try
            {
                user = await userRepository.GetUserAsync(targetId);
                byte[] pdfBytes = await policyGenerationService.GeneratePdfAsync(user!.ExtractedFields!);

                using var stream = new MemoryStream(pdfBytes);
                
                user.Status = ProcessStatus.PolicyGenerated;
                user.Policy!.Content = pdfBytes;
                user.Policy!.Status = PolicyProcessStatus.Completed;
                user.Policy.Title = "insurance_policy.pdf";
                await userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The policy was generated for User {user.UserId}",
                    Date = DateTime.UtcNow
                };
                await auditLogRepository.AddAuditLogAsync(auditLog);

                await SendPolicyAsync(stream, targetId);
            }
            catch(Exception ex)
            {
                if (user!.Policy != null)
                {
                    user.Policy.Status = PolicyProcessStatus.Failed;
                    await userRepository.SaveChangesAsync();
                }

                var error = new Error
                {
                    StackTrace = ex.StackTrace,
                    Message = $"User {targetId}" + " " + ex.Message,
                    FaildStep = FaildStep.GenerationPolicy,
                    Date = DateTime.UtcNow
                };

                await errorRepository.AddErrorAsync(error);
                throw ex;
            }
        }

        private async Task ProcessNoAsync(long chatId, User user)
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
                var aiMsg = await ReadyMessageAsync();
                await _botClient.SendMessage(chatId, aiMsg);
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

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} declined insurance price",
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _botClient.SendMessage(chatId, msg);
            }
        }

        private async Task ProcessResendPolicyAsync(long chatId, User user)
        {
            var msg = "You don't have policy yet, please call /help for support.";
            if (user.Status != ProcessStatus.PolicyGenerated)
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            msg = PolicyGeneratedMessage();
            using var stream = new MemoryStream(user.Policy!.Content!);
            await SendPolicyAsync(stream, chatId, msg);
        }

        private async Task SendPolicyAsync(MemoryStream stream, long chatId, string? msg = null)
        {
            await _botClient.SendDocument(
             chatId: chatId,
             document: Telegram.Bot.Types.InputFile.FromStream(stream, "insurance_policy.pdf"),
             caption: $"{msg}📄 Here is your insurance policy PDF.");
        }

        private Task<string> UploadVehicleRegistrationCertificateMessageAsync()
        {
            return _aIChatService.GetChatCompletionAsync("Ask user to upload vehicle registration certificate, with some guidance how to ensure the quality. Do not mention that you can't process it.");
        }
        
        private Task<string> ApprovalInsurancePolicyMessageAsync()
        {
            return _aIChatService.GetChatCompletionAsync("Say user that admin looks thowgh and answer on application, ask kindly to wait.");
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

        private async Task ProcessReadyAsync(long chatId, User? user, Telegram.Bot.Types.User telegramUser)
        {
            var aiMsg = await ReadyMessageAsync();
            if (user  == null)
            {
                user = new User
                {
                    UserId = chatId,
                    FirstName = telegramUser.FirstName,
                    LastName = telegramUser.LastName,
                    UserName = telegramUser.Username,
                    Status = ProcessStatus.Ready,
                    LastUpdated = DateTime.UtcNow,
                };
                await _userRepository.AddUserAsync(user);
            }
            else
            {
                user.Status = ProcessStatus.Ready;
                user.FirstName = telegramUser.FirstName;
                user.LastName = telegramUser.LastName;
                user.UserName = telegramUser.Username;
                user.LastUpdated = DateTime.UtcNow;

                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} is ready to start process",
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);
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
                    msg = await ApprovalInsurancePolicyMessageAsync(); 
                    break;
                case ProcessStatus.PolicyGenerated:
                    msg = PolicyGeneratedMessage();
                    using (var stream = new MemoryStream(user.Policy!.Content!))
                    await SendPolicyAsync(stream, chatId, msg);

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
                    + "/status - Check current status to continue the application\n"
                    + "/resendpolicy - Resend your policy\n";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task ProcessCancelAsync(long chatId, User user)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User requested the cancellation of the application, notify him that he will not receive insurance policy and kindly ask to try again.");
            user.Status = ProcessStatus.None;

            user.FileUploadAttempts ??= new();
            user.FileUploadAttempts!.PassportAttemps = 0;
            user.FileUploadAttempts!.VRCAttemps = 0;

            if (user.ExtractedFields != null)
            {
                var extractedFields = user.ExtractedFields;
                user.ExtractedFields = null;
                await _extractedFieldsRepository.RemoveExtractedFieldsAsync(extractedFields);
            }
            
            if(user.Policy != null)
            {
                var policy = user.Policy;
                user.Policy = null;
                await _policyRepository.RemovePolicyAsync(policy);
            }

            await _userRepository.SaveChangesAsync();

            var auditLog = new AuditLog
            {
                Message = $"The User {user.UserId} stoped the process",
                Date = DateTime.UtcNow
            };

            await _auditLogRepository.AddAuditLogAsync(auditLog);

            await _botClient.SendMessage(chatId, aiMsg);
        }

        private async Task GiveAdminCommandAsync(long chatId, User user, long targetId)
        {
            if (!user.IsAdmin)
            {
                await ProcessUnknownCommandAsync(chatId);
                return;
            }

            await _userRepository.SetAdminAsync(targetId, true);

            var msg = $"Access granted successfully to {targetId} user!";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task RevokeAdminCommandAsync(long chatId, User user, long targetId)
        {
            if (!user.IsAdmin)
            {
                await ProcessUnknownCommandAsync(chatId);
                return;
            }

            await _userRepository.SetAdminAsync(targetId, false);

            var msg = $"Access revoked successfully to {targetId} user!";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task PoliciesSummaryCommandAsync(long chatId, User user)
        {
            if (!user.IsAdmin)
            {
                await ProcessUnknownCommandAsync(chatId);
                return;
            }

            var statuses = await _policyRepository.GetStatusesAsync();
            int countOfStatuses = 0;
            int countOfInProgressStatuses = 0;
            int countOfFailedStatuses = 0;
            int countOfCompletedStatuses = 0;

            foreach(var status in statuses)
            {
                if (status == PolicyProcessStatus.InProgress)
                {
                    countOfInProgressStatuses++;
                }
                else if (status == PolicyProcessStatus.Failed)
                {
                    countOfFailedStatuses++;
                }
                else
                {
                    countOfCompletedStatuses++;
                }

                countOfStatuses++;
            }

            var msg = "Summary of issued policies:\n"
                    + $"Total issued: {countOfStatuses}\n"
                    + $"In status Completed: {countOfCompletedStatuses}\n"
                    + $"In status InProgress: {countOfInProgressStatuses}\n"
                    + $"In status Failed: {countOfFailedStatuses}\n";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task FaildPoliciesLogsCommandAsync(long chatId, User user)
        {
            if (!user.IsAdmin)
            {
                await ProcessUnknownCommandAsync(chatId);
                return;
            }

            var errors = await _errorRepository.GetPolicyErrorsAsync();
            var msg = "Faid Policies Logs:\n";
            foreach (var error in errors)
            {
                msg += $"{error}\n";
            }

            await _botClient.SendMessage(chatId, msg);
        }

        private async Task MockDocumentDataCommandAsync(long chatId, User user)
        {
            if (!user.IsAdmin)
            {
                await ProcessUnknownCommandAsync(chatId);
                return;
            }

            user.IsDocumentDataMocked = true;
            await _userRepository.SaveChangesAsync();

            var msg = $"Mock document data successfully!";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task UnMockDocumentDataCommandAsync(long chatId, User user)
        {
            if (!user.IsAdmin)
            {
                await ProcessUnknownCommandAsync(chatId);
                return;
            }

            user.IsDocumentDataMocked = false;
            await _userRepository.SaveChangesAsync();

            var msg = $"Unmock document data successfully!";
            await _botClient.SendMessage(chatId, msg);
        }
        
        private async Task GetPendingPoliciesCommandAsync(long chatId, User user)
        {
            try
            {
                if (!user.IsAdmin)
                {
                    await ProcessUnknownCommandAsync(chatId);
                    return;
                }

                var usersPolicies = await _userRepository.GetUsersPendingPoliciesAsync();
                var msg = "The list of users waiting approvals:\n\n";
                foreach (var userPolicy in usersPolicies)
                {
                    var userName = userPolicy.Surname + " " + userPolicy.GivenNames;
                    msg += $"The user number: #{userPolicy.UserId}\n" +
                        $"The user passport number: {userPolicy.PassportNumber}\n" +
                        $"The user full name: {userName}\n" +
                        $"The user birth date: {userPolicy.BirthDate}\n" +
                        $"Expiry date: {userPolicy.ExpiryDate}\n" +
                        $"The policy process status: {userPolicy.Status}\n\n";
                }

                await _botClient.SendMessage(chatId, msg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task ApprovePolicyCommandAsync(long chatId, User user, long targetId)
        {
            if (!user.IsAdmin)
            {
                await ProcessUnknownCommandAsync(chatId);
                return;
            }

            var isuUerIdExist = await _userRepository.IsUserIdExistAsync(targetId);
            if (!isuUerIdExist)
            {
                var message = $"The user {targetId} doesn't exist.";
                await _botClient.SendMessage(chatId, message);
                return;
            }

            _ = GeneratePolicyAsync(targetId);

            var msg = $"The {targetId} user was approved!";
            await _botClient.SendMessage(chatId, msg);
        }

        private async Task ProcessUnknownCommandAsync(long chatId)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User entered unknown command, notify him about it and ask to use /help command if he has any issues.");
            await _botClient.SendMessage(chatId, aiMsg);
        }
    }
}
