using CarInsuranceBot.DAL.Models.Enums;

namespace CarInsuranceBot.DAL.Models
{
    public class Policy
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public PolicyProcessStatus Status { get; set; }
        public byte[]? Content { get; set; }

        public long UserId { get; set; }
        public required User User { get; set; }
    }
}
