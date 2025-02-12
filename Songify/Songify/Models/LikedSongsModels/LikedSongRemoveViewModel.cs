namespace Songify.Models.LikedSongsModels
{
    public class LikedSongRemoveViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SongId { get; set; }
        public string SongTitle { get; internal set; }
    }
}
