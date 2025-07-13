namespace CarInsuranceBot.BLL.Enums
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
