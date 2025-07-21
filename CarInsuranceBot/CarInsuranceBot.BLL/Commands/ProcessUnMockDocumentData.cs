using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessUnMockDocumentData(IUserRepository _userRepository, ITelegramBotClient _botClient, 
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown) : IProcessUnMockDocumentData
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            if (!user.IsAdmin)
            {
                await _processUnknown.ProcessAsync(chatId);
                return;
            }

            user.IsDocumentDataMocked = false;
            await _userRepository.SaveChangesAsync();

            var msg = $"Unmock document data successfully!";
            await _botClient.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The Admin {user.UserId} unmocked document data.",
                Date = DateTime.UtcNow
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
