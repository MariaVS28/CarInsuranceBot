using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Repositories;
using CarInsuranceBot.BLL.Helpers;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessYes(ITelegramService _telegramService, IAIChatService _aIChatService, IUserRepository _userRepository, 
        IAuditLogRepository _auditLogRepository, IDateTimeHelper _dateTimeHelper) : IProcessYes
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
                var aiMsg = await _aIChatService.UploadVehicleRegistrationCertificateMessageAsync();
                user.Status = DAL.Models.Enums.ProcessStatus.PassportConfirmed;
                user.LastUpdated = _dateTimeHelper.UtcNow();
                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} confirmed passpord data",
                    Date = _dateTimeHelper.UtcNow()
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _telegramService.SendMessage(chatId, aiMsg);
            }
            else if (user.Status == DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateUploaded)
            {
                msg = await _aIChatService.InsurancePriceMessageAsync();
                user.Status = DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateConfirmed;
                user.LastUpdated = _dateTimeHelper.UtcNow();
                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} confirmed Vehicle Registration Certificate",
                    Date = _dateTimeHelper.UtcNow()
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _telegramService.SendMessage(chatId, msg);
            }
            else
            {
                var aiMsg = await _aIChatService.ApprovalInsurancePolicyMessageAsync();
                user.Status = DAL.Models.Enums.ProcessStatus.PriceAccepted;
                user.LastUpdated = _dateTimeHelper.UtcNow();
                await _userRepository.SaveChangesAsync();

                user!.Policy = new Policy
                {
                    User = user,
                    Status = PolicyProcessStatus.InProgress
                };
                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} accepted insurance price",
                    Date = _dateTimeHelper.UtcNow()
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _telegramService.SendMessage(chatId, aiMsg);
            }
        }
    }
}
