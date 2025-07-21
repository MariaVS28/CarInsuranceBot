using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessYes
    {
        Task ProcessAsync(long chatId, User user);
    }
}
