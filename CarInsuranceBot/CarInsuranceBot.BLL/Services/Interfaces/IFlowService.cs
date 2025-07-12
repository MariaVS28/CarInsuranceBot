namespace CarInsuranceBot.BLL.Services
{
    public interface IFlowService
    {
        Task ProcessTelegramCommand(long chatId, string? text);
        Task ProcessTelegramFile(long chatId, string fileId);
    }
}
