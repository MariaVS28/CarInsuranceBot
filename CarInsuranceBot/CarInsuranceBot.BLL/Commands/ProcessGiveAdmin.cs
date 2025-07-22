using CarInsuranceBot.BLL.Helpers;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessGiveAdmin(ITelegramService _telegramService, IUserRepository _userRepository,
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown, 
        IDateTimeHelper _dateTimeHelper) : IProcessGiveAdmin
    {
        public async Task ProcessAsync(long chatId, User user, long targetId)
        {
            if (!user.IsAdmin)
            {
                await _processUnknown.ProcessAsync(chatId);
                return;
            }

            var isUerIdExist = await _userRepository.IsUserIdExistAsync(targetId);
            if (!isUerIdExist)
            {
                var message = $"The user {targetId} doesn't exist.";
                await _telegramService.SendMessage(chatId, message);
                return;
            }

            await _userRepository.SetAdminAsync(targetId, true);

            var msg = $"Access granted successfully to {targetId} user!";
            await _telegramService.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The User {targetId} got access to admin right.",
                Date = _dateTimeHelper.UtcNow()
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
