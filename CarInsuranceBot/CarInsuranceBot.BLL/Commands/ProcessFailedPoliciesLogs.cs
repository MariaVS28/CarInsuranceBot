using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessFailedPoliciesLogs(ITelegramBotClient _botClient, IErrorRepository _errorRepository,
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown) : IProcessFailedPoliciesLogs
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

            await _botClient.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The Admin {user.UserId} requested faild policie's logs.",
                Date = DateTime.UtcNow
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
