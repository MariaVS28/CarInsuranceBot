using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.DAL.Repositories
{
    public class ExtractedFieldsRepository(AppDbContext _dbContext) : BaseRepository(_dbContext), IExtractedFieldsRepository
    {
        public async Task RemoveExtractedFieldsAsync(ExtractedFields extractedFields)
        {
            _dbContext.ExtractedFields.Remove(extractedFields);
            await _dbContext.SaveChangesAsync();
        }
    }
}
