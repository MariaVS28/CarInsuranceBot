using CarInsuranceBot.BLL.Services;
using Telegram.Bot;

namespace CarInsuranceBot.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            var geminiToken = Environment.GetEnvironmentVariable("GEMINI_TOKEN");
            if (telegramBotToken == null || geminiToken == null) throw new Exception("Missing environment variable.");

            services.AddSingleton<ITelegramBotClient>(provider =>
                new TelegramBotClient(telegramBotToken));

            services.AddScoped<IFlowService, FlowService>();

            services.AddHttpClient<IAIChatService, AIChatService>(client =>
            {
                client.BaseAddress = new Uri($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro-latest:generateContent?key={geminiToken}");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            
            services.AddHttpClient<ITelegramFileLoaderService, TelegramFileLoaderService>(client =>
            {
                client.BaseAddress = new Uri($"https://api.telegram.org/file/bot{telegramBotToken}/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}
