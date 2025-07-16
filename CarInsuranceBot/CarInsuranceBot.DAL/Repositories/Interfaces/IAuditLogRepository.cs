using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.DAL.Repositories
{
    public interface IAuditLogRepository : IRepository
    {
        Task AddAuditLogAsync(AuditLog auditLog);
    }
}
