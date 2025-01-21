namespace UnderGroundArchive_Backend.DTO
{
    public class ModifyBookDTO
    {
        public string BookName { get; set; }
        public int GenreId { get; set; }
        public int CategoryId { get; set; }
        public string BookDescription { get; set; }
    }
}
