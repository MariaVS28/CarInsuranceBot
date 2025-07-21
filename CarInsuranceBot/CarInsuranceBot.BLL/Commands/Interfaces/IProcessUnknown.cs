namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessUnknown
    {
        Task ProcessAsync(long chatId);
    }
}
