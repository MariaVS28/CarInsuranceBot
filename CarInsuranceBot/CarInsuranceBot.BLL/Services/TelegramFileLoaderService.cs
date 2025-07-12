namespace CarInsuranceBot.BLL.Services
{
    public class TelegramFileLoaderService(HttpClient _httpClient) : ITelegramFileLoaderService
    {
        public async Task<byte[]> DownloadTelegramFileAsync(string filePath)
        {
            var fileBytes = await _httpClient.GetByteArrayAsync(filePath);
            return fileBytes;
        }
    }
}
