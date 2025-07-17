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
                .Include(x => x.FileUploadAttempts)
                .FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task AddUserAsync(User user)
        {
            user.ExtractedFields = new();
            user.FileUploadAttempts = new();
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetAdminAsync(long targetId, bool isAdmin)
        {
            User user = new User
            {
                UserId = targetId,
                LastUpdated = DateTime.UtcNow,
                IsAdmin = isAdmin
            };

            _dbContext.Users.Attach(user);
            _dbContext.Entry(user).Property(u => u.IsAdmin).IsModified = true;
            _dbContext.Entry(user).Property(u => u.LastUpdated).IsModified = true;

            await _dbContext.SaveChangesAsync();
        }
    }
}
