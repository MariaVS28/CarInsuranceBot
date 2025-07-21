using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.DAL.Models;
using Telegram.Bot;
using CarInsuranceBot.DAL.Repositories;
using CarInsuranceBot.BLL.Services;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessReady(IAIChatService _aIChatService, ITelegramBotClient _botClient, IUserRepository _userRepository,
        IAuditLogRepository _auditLogRepository) : IProcessReady
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
                    LastUpdated = DateTime.UtcNow,
                };
                await _userRepository.AddUserAsync(user);
            }
            else
            {
                user.Status = DAL.Models.Enums.ProcessStatus.Ready;
                user.FirstName = telegramUser.FirstName;
                user.LastName = telegramUser.LastName;
                user.UserName = telegramUser.Username;
                user.LastUpdated = DateTime.UtcNow;

                await _userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The User {user.UserId} is ready to start process",
                    Date = DateTime.UtcNow
                };
                await _auditLogRepository.AddAuditLogAsync(auditLog);
            }

            await _botClient.SendMessage(chatId, aiMsg);
        }
    }
}
