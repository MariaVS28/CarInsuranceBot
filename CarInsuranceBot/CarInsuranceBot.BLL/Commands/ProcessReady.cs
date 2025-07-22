using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Repositories;
using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.BLL.Helpers;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessReady(IAIChatService _aIChatService, ITelegramService _telegramService, IUserRepository _userRepository,
        IAuditLogRepository _auditLogRepository, IDateTimeHelper _dateTimeHelper) : IProcessReady
    {
        public async Task ProcessAsync(long chatId, User? user, Telegram.Bot.Types.User telegramUser)
        {
            var aiMsg = await _aIChatService.ReadyMessageAsync();
            if (user == null)
            {
                user = new User
                {
                    UserId = chatId,
                    FirstName = telegramUser.FirstName,
                    LastName = telegramUser.LastName,
                    UserName = telegramUser.Username,
                    Status = DAL.Models.Enums.ProcessStatus.Ready,
                    LastUpdated = _dateTimeHelper.UtcNow(),
                };
                await _userRepository.AddUserAsync(user);
            }
            else
            {
                user.Status = DAL.Models.Enums.ProcessStatus.Ready;
                user.FirstName = telegramUser.FirstName;
                user.LastName = telegramUser.LastName;
                user.UserName = telegramUser.Username;
                user.LastUpdated = _dateTimeHelper.UtcNow();

                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} is ready to start process",
                    Date = _dateTimeHelper.UtcNow()
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);
            }

            await _telegramService.SendMessage(chatId, aiMsg);
        }
    }
}
