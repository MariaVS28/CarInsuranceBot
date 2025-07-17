using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.DAL.Repositories
{
    public interface IErrorRepository : IRepository
    {
        Task AddErrorAsync(Error error);
        Task<List<string>> GetPolicyErrorsAsync();
    }
}
