namespace CarInsuranceBot.BLL.Services
{
    public interface IAIChatService
    {
        Task<string> GetChatCompletionAsync(string userMessage);
        Task<string> UploadVehicleRegistrationCertificateMessageAsync();
        Task<string> ApprovalInsurancePolicyMessageAsync();
        string PolicyGeneratedMessage();
        Task<string> ReadyMessageAsync();
        Task<string> InsurancePriceMessageAsync();
    }
}
