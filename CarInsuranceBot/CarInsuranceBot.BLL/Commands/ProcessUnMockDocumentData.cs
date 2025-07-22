using CarInsuranceBot.BLL.Helpers;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessUnMockDocumentData(IUserRepository _userRepository, ITelegramService _telegramService, 
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown, 
        IDateTimeHelper _dateTimeHelper) : IProcessUnMockDocumentData
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
            await _telegramService.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The Admin {user.UserId} unmocked document data.",
                Date = _dateTimeHelper.UtcNow()
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
