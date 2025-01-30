namespace UnderGroundArchive_Backend.Models
{
    public class Requests
    {
        public int RequestId { get; set; }
        public string RequesterId { get; set; }
        public string? RequestMessage { get; set; }
        public DateTime RequestDate { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsHandled { get; set; } = false;
        public int RequestType { get; set; }
        public virtual ApplicationUser Users { get; set; }
    }
}
