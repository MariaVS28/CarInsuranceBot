using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Models.Enums;
using CarInsuranceBot.DAL.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace CarInsuranceBot.BLL.Commands 
{
    public class ProcessApprovePolicy(IUserRepository _userRepository, ITelegramBotClient _botClient,
        IAuditLogRepository _auditLogRepository, ITelegramService _telegramService, 
        IServiceScopeFactory _scopeFactory, IProcessUnknown _processUnknown) : IProcessApprovePolicy
    {
        public async Task ProcessAsync(long chatId, User user, long targetId)
        {
            if (!user.IsAdmin)
            {
                await _processUnknown.ProcessAsync(chatId);
                return;
            }

            var isuUerIdExist = await _userRepository.IsUserIdExistAsync(targetId);
            if (!isuUerIdExist)
            {
                var message = $"The user {targetId} doesn't exist.";
                await _botClient.SendMessage(chatId, message);
                return;
            }

            _ = GeneratePolicyAsync(targetId);

            var msg = $"The {targetId} user was approved!";
            await _botClient.SendMessage(chatId, msg);

            var auditLog = new AuditLog
            {
                Message = $"The Admin {user.UserId} approved policy of {targetId} user.",
                Date = DateTime.UtcNow
            };
            await _auditLogRepository.AddAuditLogAsync(auditLog);
        }

        private async Task GeneratePolicyAsync(long targetId)
        {
            using var scope = _scopeFactory.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var policyGenerationService = scope.ServiceProvider.GetRequiredService<IPolicyGenerationService>();
            var auditLogRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
            var errorRepository = scope.ServiceProvider.GetRequiredService<IErrorRepository>();

            User? user = null;
            try
            {
                user = await userRepository.GetUserAsync(targetId);
                byte[] pdfBytes = await policyGenerationService.GeneratePdfAsync(user!.ExtractedFields!);

                using var stream = new MemoryStream(pdfBytes);

                user.Status = DAL.Models.Enums.ProcessStatus.PolicyGenerated;
                user.Policy!.Content = pdfBytes;
                user.Policy!.Status = PolicyProcessStatus.Completed;
                user.Policy.Title = "insurance_policy.pdf";
                await userRepository.SaveChangesAsync();

                var auditLog = new AuditLog
                {
                    Message = $"The policy was generated for User {user.UserId}",
                    Date = DateTime.UtcNow
                };
                await auditLogRepository.AddAuditLogAsync(auditLog);

                await _telegramService.SendPolicyAsync(stream, targetId);
            }
            catch (Exception ex)
            {
                if (user!.Policy != null)
                {
                    user.Policy.Status = PolicyProcessStatus.Failed;
                    await userRepository.SaveChangesAsync();
                }

                var error = new Error
                {
                    StackTrace = ex.StackTrace,
                    Message = $"User {targetId}" + " " + ex.Message,
                    FaildStep = FaildStep.GenerationPolicy,
                    Date = DateTime.UtcNow
                };

                await errorRepository.AddErrorAsync(error);
                throw ex;
            }
        }
    }
}
