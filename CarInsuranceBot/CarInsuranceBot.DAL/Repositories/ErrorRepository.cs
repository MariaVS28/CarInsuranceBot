using CarInsuranceBot.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace CarInsuranceBot.DAL.Repositories
{
    public class ErrorRepository(AppDbContext _dbContext) : BaseRepository(_dbContext), IErrorRepository
    {
        public async Task AddErrorAsync(Error error)
        {
            await _dbContext.Errors.AddAsync(error);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<string>> GetPolicyErrorsAsync()
        {
            var policyErrors = await _dbContext.Errors
                .AsNoTracking()
                .Where(x => x.FaildStep == Models.Enums.FaildStep.GenerationPolicy)
                .Select(x => x.Message)
                .ToListAsync();

            return policyErrors;
        }
    }
}
