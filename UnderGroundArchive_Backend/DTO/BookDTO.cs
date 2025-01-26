namespace UnderGroundArchive_Backend.DTO
{
    public class BookDTO
    {
        public int Id { get; set; }
        public string BookName { get; set; }
        public int GenreId { get; set; }
        public int CategoryId { get; set; }
        public string BookDescription { get; set; }
        public IEnumerable<CommentDTO> Comments { get; set; }
        public IEnumerable<ReaderRatingDTO> ReaderRatings { get; set; }
        public IEnumerable<CriticRatingDTO> CriticRatings { get; set; }
        public IEnumerable<ChapterDTO> Chapters { get; set; }
        public string AuthorId { get; set; }
        public double AverageRating { get; set; }
    }

}
