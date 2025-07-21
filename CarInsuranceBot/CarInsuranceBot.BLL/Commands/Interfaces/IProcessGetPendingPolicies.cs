using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessGetPendingPolicies
    {
        Task ProcessAsync(long chatId, User user);
    }
}
