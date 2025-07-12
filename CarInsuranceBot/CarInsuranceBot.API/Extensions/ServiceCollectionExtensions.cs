using CarInsuranceBot.BLL.Services;
using Telegram.Bot;

namespace CarInsuranceBot.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            if (telegramBotToken != null)
            {
                services.AddSingleton<ITelegramBotClient>(provider =>
                new TelegramBotClient(telegramBotToken));
            }

            services.AddScoped<IFlowService, FlowService>();

            return services;
        }
    }
}
