using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessNo(ITelegramBotClient _botClient, IAIChatService _aIChatService, IUserRepository _userRepository,
        IAuditLogRepository _auditLogRepository) : IProcessNo
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
                var aiMsg = await _aIChatService.ReadyMessageAsync();
                await _botClient.SendMessage(chatId, aiMsg);
            }
            else if (user.Status == DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateUploaded)
            {
                var aiMsg = await _aIChatService.UploadVehicleRegistrationCertificateMessageAsync();
                await _botClient.SendMessage(chatId, aiMsg);
            }
            else
            {
                msg = await _aIChatService.InsurancePriceMessageAsync();
                user.Status = DAL.Models.Enums.ProcessStatus.PriceDeclined;
                user.LastUpdated = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} declined insurance price",
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);

                await _botClient.SendMessage(chatId, msg);
            }
        }
    }
}
