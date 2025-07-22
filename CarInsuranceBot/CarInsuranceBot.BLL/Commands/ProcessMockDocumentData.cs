using CarInsuranceBot.BLL.Helpers;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessMockDocumentData(IUserRepository _userRepository, ITelegramService _telegramService, 
        IAuditLogRepository _auditLogRepository, IProcessUnknown _processUnknown, 
        IDateTimeHelper _dateTimeHelper) : IProcessMockDocumentData
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
            await _telegramService.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The Admin {user.UserId} mocked document data.",
                Date = _dateTimeHelper.UtcNow()
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }
    }
}
