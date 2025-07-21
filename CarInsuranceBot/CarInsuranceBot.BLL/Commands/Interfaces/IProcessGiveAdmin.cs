using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessGiveAdmin
    {
        Task ProcessAsync(long chatId, User user, long targetId);
    }
}
