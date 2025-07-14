namespace CarInsuranceBot.BLL.Services
{
    public interface IMindeeService
    {
        Task<string> ParsePassportFromBytesAsync(byte[] fileBytes, string filePath);
        Task<string> ParseGenericOcrAsync(byte[] fileBytes);
        Task<string> ParseVehicleRegistrationAsync(byte[] fileBytes, string filePath);
    }
}
