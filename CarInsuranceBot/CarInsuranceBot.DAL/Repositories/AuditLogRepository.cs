using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.DAL.Repositories
{
    public class AuditLogRepository(AppDbContext _dbContext) : BaseRepository(_dbContext), IAuditLogRepository
    {
        public async Task AddAuditLogAsync(AuditLog auditLog)
        {
            await _dbContext.AuditLogs.AddAsync(auditLog);
            await _dbContext.SaveChangesAsync();
        }
    }
}
