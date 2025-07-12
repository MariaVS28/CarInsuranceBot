using CarInsuranceBot.BLL.Services;
using Telegram.Bot;

namespace CarInsuranceBot.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            if (telegramBotToken == null) throw new Exception("Missing environment variable.");

            services.AddSingleton<ITelegramBotClient>(provider =>
                new TelegramBotClient(telegramBotToken));

            services.AddScoped<IFlowService, FlowService>();

            services.AddHttpClient<IAIChatService, AIChatService>(client =>
            {
                client.BaseAddress = new Uri("https://api.openai.com/v1/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}
