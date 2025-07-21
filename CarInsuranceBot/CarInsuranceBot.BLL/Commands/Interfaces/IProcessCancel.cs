using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessCancel
    {
        Task ProcessAsync(long chatId, User user);
    }
}
