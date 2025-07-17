namespace CarInsuranceBot.BLL.Services.Interfaces
{
    public interface IDuplicateRequestDetectorService
    {
        bool IsDuplicate(long telegramUserId, string message);
    }
}
