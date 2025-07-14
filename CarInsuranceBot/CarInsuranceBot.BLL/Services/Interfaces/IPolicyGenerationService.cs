using CarInsuranceBot.BLL.Models;

namespace CarInsuranceBot.BLL.Services.Interfaces
{
    public interface IPolicyGenerationService
    {
        Task<byte[]> GeneratePdfAsync(ExtractedFields extractedFields);
    }
}
