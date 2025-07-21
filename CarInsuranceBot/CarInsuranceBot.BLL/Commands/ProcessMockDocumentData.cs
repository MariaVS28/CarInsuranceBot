using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessMockDocumentData(IUserRepository _userRepository, ITelegramBotClient _botClient, 
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown) : IProcessMockDocumentData
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            if (!user.IsAdmin)
            {
                await _processUnknown.ProcessAsync(chatId);
                return;
            }

            user.IsDocumentDataMocked = true;
            await _userRepository.SaveChangesAsync();

            var msg = $"Mock document data successfully!";
            await _botClient.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The Admin {user.UserId} mocked document data.",
                Date = DateTime.UtcNow
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
