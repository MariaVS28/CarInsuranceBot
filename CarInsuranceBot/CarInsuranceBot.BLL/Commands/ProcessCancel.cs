using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.DAL.Models;
using Telegram.Bot;
using CarInsuranceBot.DAL.Repositories;
using CarInsuranceBot.BLL.Services;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessCancel(IAIChatService _aIChatService, ITelegramBotClient _botClient, IUserRepository _userRepository,
        IAuditLogRepository _auditLogRepository, IExtractedFieldsRepository _extractedFieldsRepository,
        IPolicyRepository _policyRepository) : IProcessCancel
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            var aiMsg = await _aIChatService.GetChatCompletionAsync("User requested the cancellation of the application, notify him that he will not receive insurance policy and kindly ask to try again.");
            user.Status = DAL.Models.Enums.ProcessStatus.None;

            user.FileUploadAttempts ??= new();
            user.FileUploadAttempts!.PassportAttemps = 0;
            user.FileUploadAttempts!.VRCAttemps = 0;

            if (user.ExtractedFields != null)
            {
                var extractedFields = user.ExtractedFields;
                user.ExtractedFields = null;
                await _extractedFieldsRepository.RemoveExtractedFieldsAsync(extractedFields);
            }

            if (user.Policy != null)
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
    }
}
