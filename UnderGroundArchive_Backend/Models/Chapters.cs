using System.Text.Json.Serialization;

namespace UnderGroundArchive_Backend.Models
{
    public class Chapters
    {
        public int ChapterId { get; set; }
        public int BookId { get; set; }
        public int ChapterNumber { get; set; }
        public string ChapterTitle { get; set; }
        public string ChapterContent { get; set; }
        [JsonIgnore]
        public virtual Books Book { get; set; }
    }
}
