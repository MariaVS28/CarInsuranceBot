namespace CarInsuranceBot.DAL.Models.Enums
{
    public enum ProcessStatus
    {
        None,
        Ready, 
        PassportUploaded,
        PassportConfirmed,
        VehicleRegistrationCertificateUploaded,
        VehicleRegistrationCertificateConfirmed,
        PriceAccepted,
        PriceDeclined,
        PolicyGenerated
    }
}
