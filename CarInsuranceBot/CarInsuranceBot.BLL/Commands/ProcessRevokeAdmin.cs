
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessRevokeAdmin(ITelegramBotClient _botClient, IUserRepository _userRepository,
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown) : IProcessRevokeAdmin
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
                await _botClient.SendMessage(chatId, message);
                return;
            }

            await _userRepository.SetAdminAsync(targetId, false);

            var msg = $"Access revoked successfully to {targetId} user!";
            await _botClient.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The User {user.UserId} lost access to admin right.",
                Date = DateTime.UtcNow
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
