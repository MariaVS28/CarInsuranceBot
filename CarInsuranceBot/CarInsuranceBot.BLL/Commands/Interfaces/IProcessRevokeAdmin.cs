using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessRevokeAdmin
    {
        Task ProcessAsync(long chatId, User user, long targetId);
    }
}
