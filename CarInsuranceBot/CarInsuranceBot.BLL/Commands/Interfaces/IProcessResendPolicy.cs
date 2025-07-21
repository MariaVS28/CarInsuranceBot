using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessResendPolicy
    {
        Task ProcessAsync(long chatId, User user);
    }
}
