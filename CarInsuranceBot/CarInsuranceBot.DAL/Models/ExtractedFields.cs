namespace CarInsuranceBot.DAL.Models
{
    public class ExtractedFields
    {
        public int Id { get; set; }
        public string? PassportNumber { get; set; }
        public string? Surname { get; set; }
        public string? GivenNames { get; set; }
        public string? BirthDate { get; set; }
        public string? ExpiryDate { get; set; }

        public DateTime? VehiclesRegistrationDate { get; set; }
        public string? VehicleIdentificationNumber { get; set; }
        public string? VehicleOwnersFullName { get; set; }
        public string? VehicleMake { get; set; }
        public string? VehicleModel { get; set; }

        public long UserId { get; set; }
        public User? User { get; set; }
    }
}
