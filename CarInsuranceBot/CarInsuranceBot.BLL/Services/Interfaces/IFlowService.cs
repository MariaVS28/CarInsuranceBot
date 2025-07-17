namespace CarInsuranceBot.BLL.Services
{
    public interface IFlowService
    {
        Task ProcessTelegramCommandAsync(long chatId, string? text, Telegram.Bot.Types.User telegramUser);
        Task ProcessTelegramFileAsync(long chatId, string fileId);
    }
}
