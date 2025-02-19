namespace UnderGroundArchive_Backend.Models
{
    public class Favourites
    {
        public int FavouriteId { get; set; }
        public string UserId { get; set; }
        public int BookId { get; set; }
        public int? ChapterId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Books Book { get; set; }
        public virtual Chapters LastReadChapter { get; set; }
    }
}
