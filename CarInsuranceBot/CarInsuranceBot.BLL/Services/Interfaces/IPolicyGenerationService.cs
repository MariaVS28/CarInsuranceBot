using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Services
{
    public interface IPolicyGenerationService
    {
        Task<byte[]> GeneratePdfAsync(ExtractedFields extractedFields);
    }
}
