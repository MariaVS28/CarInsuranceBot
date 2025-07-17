namespace CarInsuranceBot.DAL.Models
{
    public class FileUploadAttempt
    {
        public long Id { get; set; }
        public int PassportAttemps { get; set; }
        public int VRCAttemps { get; set; }

        public long UserId { get; set; }
        public User? User { get; set; }
    }
}
