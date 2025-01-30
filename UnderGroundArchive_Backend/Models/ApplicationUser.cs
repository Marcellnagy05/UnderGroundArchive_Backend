using Microsoft.AspNetCore.Identity;

namespace UnderGroundArchive_Backend.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? RankId { get; set; } = 1;
        public int? SubscriptionId { get; set; } = 1;
        public DateTime JoinDate { get; set; } = DateTime.Now;
        public DateTime? BirthDate { get; set; }
        public string? Country { get; set; }
        public int RankPoints { get; set; } = 0;
        public decimal Balance { get; set; } = 0;
        public string Favourites { get; set; } = "";
        public string Theme { get; set; } = "dark";
        public bool IsMuted { get; set; } = false;
        public bool IsBanned { get; set; } = false;
        public virtual ICollection<Books> Books { get; set; } = new List<Books>();
        public virtual ICollection<Requests> Requests { get; set; } = new List<Requests>();
        public virtual ICollection<CompletedAchievements> CompletedAchievements { get; set; } = new List<CompletedAchievements>();
        public virtual ICollection<Comments> Comments { get; set; } = new List<Comments>();
        public virtual ICollection<Reports> ReportSender { get; set; } = new List<Reports>();
        public virtual ICollection<Reports> ReportSubject { get; set; } = new List<Reports>();
    }
}