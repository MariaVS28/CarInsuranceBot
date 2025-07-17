using CarInsuranceBot.DAL.Models.Enums;

namespace CarInsuranceBot.DAL.Models
{
    public class User
    {
        public long UserId { get; set; }
        public ProcessStatus Status { get; set; }
        public DateTime LastUpdated { get; set; }

        public ExtractedFields? ExtractedFields { get; set; }
        public List<Document> Documents { get; set; } = [];
        public Policy? Policy { get; set; }
        public FileUploadAttempt? FileUploadAttempts { get; set; }
    }
}
