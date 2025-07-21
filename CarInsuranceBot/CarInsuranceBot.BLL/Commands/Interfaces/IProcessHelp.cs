namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessHelp
    {
        Task ProcessAsync(long chatId);
    }
}
