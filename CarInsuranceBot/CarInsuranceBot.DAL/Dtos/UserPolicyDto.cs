using CarInsuranceBot.DAL.Models.Enums;

namespace CarInsuranceBot.DAL.Dtos
{
    public class UserPolicyDto
    {
        public long UserId { get; set; }
        public string? PassportNumber { get; set; }
        public string? Surname { get; set; }
        public string? GivenNames { get; set; }
        public string? BirthDate { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Status { get; set; }
    }
}
