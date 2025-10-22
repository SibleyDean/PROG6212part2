namespace CMCS.Mvc.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Hours { get; set; }
        public decimal Rate { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string LecturerName { get; set; } = string.Empty;

        // Computed property for convenience
        public decimal Amount => Hours * Rate;
    }
}