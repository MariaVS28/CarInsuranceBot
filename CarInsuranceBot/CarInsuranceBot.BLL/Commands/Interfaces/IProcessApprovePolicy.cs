using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessApprovePolicy
    {
        Task ProcessAsync(long chatId, User user, long targetId);
    }
}
