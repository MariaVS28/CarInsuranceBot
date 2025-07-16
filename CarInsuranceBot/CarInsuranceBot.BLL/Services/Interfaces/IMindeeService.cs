using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Services
{
    public interface IMindeeService
    {
        Task<string> ParsePassportFromBytesAsync(long chatId, byte[] fileBytes, string filePath, User user);
        Task<string> ParseVehicleRegistrationAsync(long chatId, byte[] fileBytes, string filePath, User user);
    }
}
