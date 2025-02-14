namespace Songify.Entities
{
    public class LikedSong
    {
        public string UserId { get; set; }
        public SongifyUser SongifyUser { get; set; }
        public int SongId { get; set; }
        public Song Song { get; set; }
    }
}
