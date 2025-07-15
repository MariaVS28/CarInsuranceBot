using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Services.Interfaces
{
    public interface IPolicyGenerationService
    {
        Task<byte[]> GeneratePdfAsync(ExtractedFields extractedFields);
    }
}
