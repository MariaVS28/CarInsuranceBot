using CarInsuranceBot.DAL.Models;
using CarInsuranceBot.DAL.Models.Enums;

namespace CarInsuranceBot.DAL.Repositories
{
    public interface IPolicyRepository : IRepository
    {
        Task<List<PolicyProcessStatus>> GetStatusesAsync();
        Task RemovePolicyAsync(Policy policy);
    }
}
