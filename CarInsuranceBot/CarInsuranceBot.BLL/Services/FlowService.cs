using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.DAL.Models;
using Telegram.Bot;
using CarInsuranceBot.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using CarInsuranceBot.BLL.Commands;
using ProcessStatus = CarInsuranceBot.DAL.Models.Enums.ProcessStatus;

namespace CarInsuranceBot.BLL.Services
{
    public class FlowService(ITelegramBotClient _botClient, IAIChatService _aIChatService, 
        ITelegramFileLoaderService _telegramFileLoaderService, IMindeeService _mindeeService, 
        IUserRepository _userRepository, IAuditLogRepository _auditLogRepository, 
        IErrorRepository _errorRepository, IServiceProvider _serviceProvider) : IFlowService
    {
        private readonly HashSet<ProcessStatus> _processStatusesToUploadFile = [ProcessStatus.Ready, ProcessStatus.PassportUploaded, ProcessStatus.PassportConfirmed, ProcessStatus.VehicleRegistrationCertificateUploaded];

        public async Task ProcessTelegramCommandAsync(long chatId, string? text, Telegram.Bot.Types.User telegramUser)
        {
            var user = await _userRepository.GetUserAsync(chatId);
            if (user == null 
                && (text != "/start" && text != "/help" && text != "/ready" && text != "/status"))
            {
                await _serviceProvider.GetRequiredService<IProcessUnknown>().ProcessAsync(chatId);
                return;
            }

            long targetId = 0;

            if (text!.StartsWith($"/giveadmin"))
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
                    await _serviceProvider.GetRequiredService<IProcessStart>().ProcessAsync(chatId);
                    break;
                case "/help":
                    await _serviceProvider.GetRequiredService<IProcessHelp>().ProcessAsync(chatId);
                    break;
                case "/ready":
                    await _serviceProvider.GetRequiredService<IProcessReady>().ProcessAsync(chatId, user, telegramUser);
                    break;
                case "/status":
                    await _serviceProvider.GetRequiredService<IProcessStatus>().ProcessAsync(chatId, user);
                    break;
                case "/cancel":
                    await _serviceProvider.GetRequiredService<IProcessCancel>().ProcessAsync(chatId, user!);
                    break;
                case "/yes":
                    await _serviceProvider.GetRequiredService<IProcessYes>().ProcessAsync(chatId, user!);
                    break;
                case "/no":
                    await _serviceProvider.GetRequiredService<IProcessNo>().ProcessAsync(chatId, user!);
                    break;
                case "/resendpolicy":
                    await _serviceProvider.GetRequiredService<IProcessResendPolicy>().ProcessAsync(chatId, user!);
                    break;
                case "/giveadmin":
                    await _serviceProvider.GetRequiredService<IProcessGiveAdmin>().ProcessAsync(chatId, user!, targetId);
                    break;
                case "/revokeadmin":
                    await _serviceProvider.GetRequiredService<IProcessRevokeAdmin>().ProcessAsync(chatId, user!, targetId);
                    break;
                case "/policiessummary":
                    await _serviceProvider.GetRequiredService<IProcessPoliciesSummary>().ProcessAsync(chatId, user!);
                    break;
                case "/failedpolicieslogs":
                    await _serviceProvider.GetRequiredService<IProcessFailedPoliciesLogs>().ProcessAsync(chatId, user!);
                    break;
                case "/mockdocumentdata":
                    await _serviceProvider.GetRequiredService<IProcessMockDocumentData>().ProcessAsync(chatId, user!);
                    break;
                case "/unmockdocumentdata":
                    await _serviceProvider.GetRequiredService<IProcessUnMockDocumentData>().ProcessAsync(chatId, user!);
                    break;
                case "/getpendingpolicies":
                    await _serviceProvider.GetRequiredService<IProcessGetPendingPolicies>().ProcessAsync(chatId, user!);
                    break;
                case "/approvepolicy":
                    await _serviceProvider.GetRequiredService<IProcessApprovePolicy>().ProcessAsync(chatId, user!, targetId);
                    break;
                default:
                    await _serviceProvider.GetRequiredService<IProcessUnknown>().ProcessAsync(chatId);
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
    }
}
