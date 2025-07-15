namespace CarInsuranceBot.DAL.Models
{
    public class AuditLog
    {
        public long Id { get; set; }
        public required string Message { get; set; }
        public DateTime Date { get; set; }
    }
}
