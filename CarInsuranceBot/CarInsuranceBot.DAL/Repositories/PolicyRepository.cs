using CarInsuranceBot.DAL.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CarInsuranceBot.DAL.Repositories
{
    public class PolicyRepository(AppDbContext _dbContext) : BaseRepository(_dbContext), IPolicyRepository
    {
        public async Task<List<PolicyProcessStatus>> GetStatusesAsync()
        {
            var statuses = await _dbContext.Policies
                .AsNoTracking()
                .Select(x => x.Status)
                .ToListAsync();

            return statuses;
        }
    }
}
