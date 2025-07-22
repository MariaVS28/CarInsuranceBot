namespace CarInsuranceBot.BLL.Helpers
{
    public class DateTimeHelper : IDateTimeHelper
    {
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
