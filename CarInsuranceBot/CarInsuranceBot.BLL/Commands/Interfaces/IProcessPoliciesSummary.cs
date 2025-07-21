using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessPoliciesSummary
    {
        Task ProcessAsync(long chatId, User user);
    }
}
