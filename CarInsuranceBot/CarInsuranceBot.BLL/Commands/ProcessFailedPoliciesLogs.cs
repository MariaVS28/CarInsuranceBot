using CarInsuranceBot.BLL.Helpers;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessFailedPoliciesLogs(ITelegramService _telegramService, IErrorRepository _errorRepository,
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown, 
        IDateTimeHelper _dateTimeHelper) : IProcessFailedPoliciesLogs
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            if (!user.IsAdmin)
            {
                await _processUnknown.ProcessAsync(chatId);
                return;
            }

            var errors = await _errorRepository.GetPolicyErrorsAsync();
            var msg = "Faid Policies Logs:\n";
            foreach (var error in errors)
            {
                msg += $"{error}\n";
            }

            await _telegramService.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The Admin {user.UserId} requested faild policie's logs.",
                Date = _dateTimeHelper.UtcNow()
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
