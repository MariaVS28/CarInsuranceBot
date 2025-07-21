using CarInsuranceBot.DAL.Dtos;
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

        public async Task<List<UserPolicyDto>> GetUsersPendingPoliciesAsync()
        {
            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Policy != null && 
                (x.Policy.Status == Models.Enums.PolicyProcessStatus.InProgress 
                || x.Policy.Status == Models.Enums.PolicyProcessStatus.Failed))
                .Select(x => new UserPolicyDto
                {
                    UserId = x.UserId,
                    PassportNumber = x.ExtractedFields != null ? x.ExtractedFields.PassportNumber : string.Empty,
                    Surname = x.ExtractedFields != null ? x.ExtractedFields.Surname : string.Empty,
                    GivenNames = x.ExtractedFields != null ? x.ExtractedFields.GivenNames : string.Empty,
                    BirthDate = x.ExtractedFields != null ? x.ExtractedFields.BirthDate : string.Empty,
                    ExpiryDate = x.ExtractedFields != null ? x.ExtractedFields.ExpiryDate : string.Empty,
                    Status = x.Policy != null ? x.Policy.Status.ToString() : string.Empty,
                })
                .ToListAsync();

            return users;
        }

        public async Task<bool> IsUserIdExistAsync(long userId)
        {
            return await _dbContext.Users
                .AnyAsync(x => x.UserId == userId);
        }
    }
}
