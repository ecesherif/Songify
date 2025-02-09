namespace Songify.Entities
{
    public class LikedSong
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public SongifyUser SongifyUser { get; set; }
        public int SongId { get; set; }
        public Song Song { get; set; }
    }
}
