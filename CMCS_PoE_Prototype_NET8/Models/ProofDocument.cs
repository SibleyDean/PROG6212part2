using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CMCS.Mvc.Models
{
    public class ProofDocument
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public int? ClaimId { get; set; }
        public Claim? Claim { get; set; }
    }
}
