using CarInsuranceBot.DAL.Models.Enums;

namespace CarInsuranceBot.DAL.Models
{
    public class Error
    {
        public long Id { get; set; }
        public required string Message { get; set; }
        public required string StackTrace { get; set; }
        public FaildStep FaildStep { get; set; }
        public DateTime Date { get; set; }
    }
}
