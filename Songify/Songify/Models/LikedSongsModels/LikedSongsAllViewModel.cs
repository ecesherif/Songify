namespace Songify.Models.LikedSongsModels
{
    public class LikedSongsAllViewModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string SongId { get; set; }
        public string SongTitle { get; internal set; }
    }
}
