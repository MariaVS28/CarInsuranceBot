namespace CarInsuranceBot.BLL.Services
{
    public interface ITelegramFileLoaderService
    {
        Task<byte[]> DownloadTelegramFileAsync(string filePath);
    }
}
