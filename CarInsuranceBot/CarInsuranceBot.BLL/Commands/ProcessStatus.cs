using CarInsuranceBot.BLL.Services;
using CarInsuranceBot.DAL.Models;

namespace CarInsuranceBot.BLL.Commands
{
    public class ProcessStatus(IAIChatService _aIChatService, ITelegramService _telegramService) : IProcessStatus
    {
        public async Task ProcessAsync(long chatId, User? user)
        {
            var msg = "Process wasn't started.";
            if (user == null)
            {
                await _telegramService.SendMessage(chatId, msg);
                return;
            }

            switch (user.Status)
            {
                case DAL.Models.Enums.ProcessStatus.Ready:
                    msg = await _aIChatService.ReadyMessageAsync();
                    break;
                case DAL.Models.Enums.ProcessStatus.PassportUploaded:
                    msg = "Passport was uploaded, please confirm the data:\n" +
                       $"Passport Number: {user.ExtractedFields?.PassportNumber}\n" +
                       $"Surname: {user.ExtractedFields?.Surname}\n" +
                       $"Given Names: {string.Join(" ", user.ExtractedFields?.GivenNames)}\n" +
                       $"Date of Birth: {user.ExtractedFields?.BirthDate}\n" +
                       $"Expiry Date: {user.ExtractedFields?.ExpiryDate}";
                    break;
                case DAL.Models.Enums.ProcessStatus.PassportConfirmed:
                    msg = await _aIChatService.UploadVehicleRegistrationCertificateMessageAsync();
                    break;
                case DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateUploaded:
                    msg = "Vehicle registration certificate was uploaded, please confirm the data:\n" +
                         $"Vehicle Owner's Full Name: {user.ExtractedFields?.VehicleOwnersFullName}\n" +
                         $"Vehicle's Registration Date: {user.ExtractedFields?.VehiclesRegistrationDate}\n" +
                         $"Vehicle Identification Number: {user.ExtractedFields?.VehicleIdentificationNumber}\n" +
                         $"Vehicle Make: {user.ExtractedFields?.VehicleMake}\n" +
                         $"Vehicle Model: {user.ExtractedFields?.VehicleModel}";
                    break;
                case DAL.Models.Enums.ProcessStatus.VehicleRegistrationCertificateConfirmed:
                case DAL.Models.Enums.ProcessStatus.PriceDeclined:
                    msg = await _aIChatService.InsurancePriceMessageAsync();
                    break;
                case DAL.Models.Enums.ProcessStatus.PriceAccepted:
                    msg = await _aIChatService.ApprovalInsurancePolicyMessageAsync();
                    break;
                case DAL.Models.Enums.ProcessStatus.PolicyGenerated:
                    msg = _aIChatService.PolicyGeneratedMessage();
                    using (var stream = new MemoryStream(user.Policy!.Content!))
                        await _telegramService.SendPolicyAsync(stream, chatId, msg);

                    break;
            }
            await _telegramService.SendMessage(chatId, msg);
        }
    }
}
