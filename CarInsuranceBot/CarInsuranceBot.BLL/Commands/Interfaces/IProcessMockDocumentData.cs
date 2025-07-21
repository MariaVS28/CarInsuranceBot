using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessMockDocumentData
    {
        Task ProcessAsync(long chatId, User user);
    }
}
