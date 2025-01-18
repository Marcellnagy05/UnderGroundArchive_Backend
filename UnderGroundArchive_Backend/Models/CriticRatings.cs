namespace UnderGroundArchive_Backend.Models
{
    public class CriticRatings
    {
        public int RatingId { get; set; }
        public int BookId { get; set; }
        public int RatingValue { get; set; }
        public int RaterId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ModifiedAt { get; set; }
        public string? RatingDescription { get; set; }
        public Books Books { get; set; }
    }
}
