using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessGetPendingPolicies(IUserRepository _userRepository, ITelegramBotClient _botClient,
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown) : IProcessGetPendingPolicies
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            if (!user.IsAdmin)
            {
                await _processUnknown.ProcessAsync(chatId);
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

            var auditLog = new AuditLog
            {
                Message = $"The Admin {user.UserId} requested policies in pending.",
                Date = DateTime.UtcNow
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
