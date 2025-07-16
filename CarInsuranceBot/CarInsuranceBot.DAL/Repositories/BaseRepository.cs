namespace CarInsuranceBot.DAL.Repositories
{
    public abstract class BaseRepository(AppDbContext _dbContext) : IRepository
    {
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
