namespace CarInsuranceBot.BLL.Services
{
    public interface IMindeeService
    {
        Task<string> ParsePassportFromBytesAsync(long chatId, byte[] fileBytes, string filePath);
        Task<string> ParseVehicleRegistrationAsync(long chatId, byte[] fileBytes);
    }
}
