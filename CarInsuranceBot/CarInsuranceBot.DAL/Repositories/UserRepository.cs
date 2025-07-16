using CarInsuranceBot.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsuranceBot.DAL.Repositories
{
    public class UserRepository(AppDbContext _dbContext) : BaseRepository(_dbContext), IUserRepository
    {
        public async Task<User?> GetUserAsync(long id)
        {
            return await _dbContext.Users
                .Include(x => x.ExtractedFields)
                .Include(x => x.Documents)
                .Include(x => x.Policy)
                .FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task AddUserAsync(User user)
        {
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }
    }
}
