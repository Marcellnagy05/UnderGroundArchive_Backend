namespace UnderGroundArchive_Backend.Models
{
    public class Reports
    {
        public int ReportId { get; set; }
        public string ReporterId { get; set; }
        public string ReportedId { get; set; }
        public int ReportTypeId { get; set; }
        public string? ReportMessage { get; set; }
        public bool IsHandled { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ReportTypes ReportTypes { get; set; }
        public virtual ApplicationUser ReporterPeople { get; set; }
        public virtual ApplicationUser ReportedPeople { get; set; }

    }
}
