namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessStart
    {
        Task ProcessAsync(long chatId);
    }
}
