using CarInsuranceBot.DAL.Models.Enums;

namespace CarInsuranceBot.DAL.Repositories
{
    public interface IPolicyRepository : IRepository
    {
        Task<List<PolicyProcessStatus>> GetStatusesAsync();
    }
}
