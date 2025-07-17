namespace CarInsuranceBot.DAL.Models.Enums
{
    public enum FaildStep
    {
        General,
        ProcessFile,
        ParseVRCData,
        ParsePassportData,
        ChatCompletion,
        GenerationPolicy
    }
}
