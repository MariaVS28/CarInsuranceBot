using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public interface IProcessUnMockDocumentData
    {
        Task ProcessAsync(long chatId, User user);
    }
}
