namespace Songify.Entities
{
    public class LikedSong
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public SongifyUser SongifyUser { get; set; }
        public int SongId { get; set; }
        public Song Song { get; set; }
        public ICollection<Song> Songs { get; set; } = new List<Song>();
        public ICollection<SongifyUser> Users { get; set; } = new List<SongifyUser>();
    }
}
