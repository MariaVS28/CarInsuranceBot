using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.BLL.Services.Interfaces;
using CarInsuranceBot.DAL;
using CarInsuranceBot.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Mindee;
using QuestPDF.Infrastructure;
using Telegram.Bot;

namespace CarInsuranceBot.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var telegramBotToken = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
            var geminiToken = Environment.GetEnvironmentVariable("GEMINI_TOKEN");
            var mindeeToken = Environment.GetEnvironmentVariable("MINDEE_KEY");
            var azureDbPassword = Environment.GetEnvironmentVariable("AZURE_DB_PASSWORD");
            if (telegramBotToken == null || geminiToken == null || mindeeToken == null || azureDbPassword == null) throw new Exception("Missing environment variable.");

            services.AddSingleton<ITelegramBotClient>(provider =>
                new TelegramBotClient(telegramBotToken));

            services.AddScoped<IFlowService, FlowService>();
            services.AddScoped<IPolicyGenerationService, PolicyGenerationService>();
            services.AddSingleton<IDuplicateRequestDetectorService, DuplicateRequestDetectorService>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddScoped<IErrorRepository, ErrorRepository>();
            services.AddScoped<IPolicyRepository, PolicyRepository>();

            QuestPDF.Settings.License = LicenseType.Community;

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

            services.AddSingleton(new MindeeClient(mindeeToken));
            services.AddHttpClient<IMindeeService, MindeeService>(client =>
            {
                client.BaseAddress = new Uri("https://api.mindee.net/"); 
                client.DefaultRequestHeaders.Add("Authorization", $"Token {mindeeToken}");
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    AllowAutoRedirect = false
                };
            });

            var connectionString = configuration.GetConnectionString("AzureDb");
            connectionString = connectionString!.Replace("[Password]", azureDbPassword);
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            return services;
        }
    }
}
