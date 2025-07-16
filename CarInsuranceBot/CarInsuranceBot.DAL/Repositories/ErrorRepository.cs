using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.DAL.Repositories
{
    public class ErrorRepository(AppDbContext _dbContext) : BaseRepository(_dbContext), IErrorRepository
    {
        public async Task AddErrorAsync(Error error)
        {
            await _dbContext.Errors.AddAsync(error);
            await _dbContext.SaveChangesAsync();
        }
    }
}
