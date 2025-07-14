using CarInsuranceBot.BLL.Models;

namespace CarInsuranceBot.BLL.Services.Interfaces
{
    public interface IPolicyGenerationService
    {
        byte[] GeneratePdf(ExtractedFields extractedFields);
    }
}
