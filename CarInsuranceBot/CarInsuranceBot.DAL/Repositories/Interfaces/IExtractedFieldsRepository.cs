using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.DAL.Repositories
{
    public interface IExtractedFieldsRepository : IRepository
    {
        Task RemoveExtractedFieldsAsync(ExtractedFields extractedFields);
    }
}
