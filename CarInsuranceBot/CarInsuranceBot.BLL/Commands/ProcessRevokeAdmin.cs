
using CarInsuranceBot.BLL.Helpers;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessRevokeAdmin(ITelegramService _telegramService, IUserRepository _userRepository,
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown, 
        IDateTimeHelper _dateTimeHelper) : IProcessRevokeAdmin
    {
        public async Task ProcessAsync(long chatId, User user, long targetId)
        {
            if (!user.IsAdmin)
            {
                await _processUnknown.ProcessAsync(chatId);
                return;
            }

            var isuUerIdExist = await _userRepository.IsUserIdExistAsync(targetId);
            if (!isuUerIdExist)
            {
                var message = $"The user {targetId} doesn't exist.";
                await _telegramService.SendMessage(chatId, message);
                return;
            }

            await _userRepository.SetAdminAsync(targetId, false);

            var msg = $"Access revoked successfully to {targetId} user!";
            await _telegramService.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The User {targetId} lost access to admin right.",
                Date = _dateTimeHelper.UtcNow()
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
