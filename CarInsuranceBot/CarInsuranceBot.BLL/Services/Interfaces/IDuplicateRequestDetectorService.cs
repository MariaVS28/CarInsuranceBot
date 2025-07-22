namespace CarInsuranceBot.BLL.Services
{
    public interface IDuplicateRequestDetectorService
    {
        bool IsDuplicate(long telegramUserId, string message);
    }
}
