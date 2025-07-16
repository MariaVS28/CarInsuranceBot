using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.DAL.Repositories
{
    public interface IUserRepository : IRepository
    {
        Task<User?> GetUserAsync(long id);
        Task AddUserAsync(User user);
    }
}
