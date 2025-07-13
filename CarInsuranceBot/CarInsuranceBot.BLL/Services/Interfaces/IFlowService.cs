namespace CarInsuranceBot.BLL.Services
{
    public interface IFlowService
    {
        Task ProcessTelegramCommandAsync(long chatId, string? text);
        Task ProcessTelegramFileAsync(long chatId, string fileId);
    }
}
