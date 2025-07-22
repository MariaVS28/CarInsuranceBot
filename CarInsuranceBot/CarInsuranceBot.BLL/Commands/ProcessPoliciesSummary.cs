using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.BLL.Helpers;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessPoliciesSummary(ITelegramService _telegramService, IAuditLogRepository _auditLogRepository, 
        IPolicyRepository _policyRepository, IProcessUnknown _processUnknown, 
        IDateTimeHelper _dateTimeHelper) : IProcessPoliciesSummary
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            if (!user.IsAdmin)
            {
                await _processUnknown.ProcessAsync(chatId);
                return;
            }

            var statuses = await _policyRepository.GetStatusesAsync();
            int countOfStatuses = 0;
            int countOfInProgressStatuses = 0;
            int countOfFailedStatuses = 0;
            int countOfCompletedStatuses = 0;

            foreach (var status in statuses)
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
            await _telegramService.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The Admin {user.UserId} requested summary of policies.",
                Date = _dateTimeHelper.UtcNow()
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
