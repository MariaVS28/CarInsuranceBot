using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessNo
    {
        Task ProcessAsync(long chatId, User user);
    }
}
