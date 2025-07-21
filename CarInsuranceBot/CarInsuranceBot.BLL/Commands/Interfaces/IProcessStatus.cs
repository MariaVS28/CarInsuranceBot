using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessStatus
    {
        Task ProcessAsync(long chatId, User? user);
    }
}
