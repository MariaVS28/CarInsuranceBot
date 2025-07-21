using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.DAL.Models;
using Telegram.Bot;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Repositories;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessYes(ITelegramBotClient _botClient, IAIChatService _aIChatService, IUserRepository _userRepository, 
        IAuditLogRepository _auditLogRepository) : IProcessYes
    {
        public async Task ProcessAsync(long chatId, User user)
        {
            var msg = "Please follow the instructions or call /help for support.";
            if (!(user.Status == DAL.Models.Enums.ProcessStatus.PassportUploaded || user.Status == DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateUploaded
                || user.Status == DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateConfirmed || user.Status == DAL.Models.Enums.ProcessStatus.PriceDeclined))
            {
                await _botClient.SendMessage(chatId, msg);
                return;
            }

            if (user.Status == DAL.Models.Enums.ProcessStatus.PassportUploaded)
            {
                var aiMsg = await _aIChatService.UploadVehicleRegistrationCertificateMessageAsync();
                user.Status = DAL.Models.Enums.ProcessStatus.PassportConfirmed;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} confirmed passpord data",
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _botClient.SendMessage(chatId, aiMsg);
            }
            else if (user.Status == DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateUploaded)
            {
                msg = await _aIChatService.InsurancePriceMessageAsync();
                user.Status = DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateConfirmed;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} confirmed Vehicle Registration Certificate",
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _botClient.SendMessage(chatId, msg);
            }
            else
            {
                var aiMsg = await _aIChatService.ApprovalInsurancePolicyMessageAsync();
                user.Status = DAL.Models.Enums.ProcessStatus.PriceAccepted;
                user.LastUpdated = DateTime.UtcNow;
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
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _botClient.SendMessage(chatId, aiMsg);
            }
        }
    }
}
