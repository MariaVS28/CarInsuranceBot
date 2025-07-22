using CarInsuranceBot.BLL.Helpers;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessNo(ITelegramService _telegramService, IAIChatService _aIChatService, IUserRepository _userRepository,
        IAuditLogRepository _auditLogRepository, IDateTimeHelper _dateTimeHelper) : IProcessNo
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            var msg = "Please follow the instructions or call /help for support.";
            if (!(user.Status == DAL.Models.Enums.ProcessStatus.PassportUploaded || user.Status == DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateUploaded
                || user.Status == DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateConfirmed || user.Status == DAL.Models.Enums.ProcessStatus.PriceDeclined))
            {
                await _telegramService.SendMessage(chatId, msg);
                return;
            }

            if (user.Status == DAL.Models.Enums.ProcessStatus.PassportUploaded)
            {
                var aiMsg = await _aIChatService.ReadyMessageAsync();
                await _telegramService.SendMessage(chatId, aiMsg);
            }
            else if (user.Status == DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateUploaded)
            {
                var aiMsg = await _aIChatService.UploadVehicleRegistrationCertificateMessageAsync();
                await _telegramService.SendMessage(chatId, aiMsg);
            }
            else
            {
                msg = await _aIChatService.InsurancePriceMessageAsync();
                user.Status = DAL.Models.Enums.ProcessStatus.PriceDeclined;
                user.LastUpdated = _dateTimeHelper.UtcNow();
                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} declined insurance price",
                    Date = _dateTimeHelper.UtcNow()
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _telegramService.SendMessage(chatId, msg);
            }
        }
    }
}
