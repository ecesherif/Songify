namespace Songify.Models.SongModels
{
    public class SongDeleteViewModel
    {
        public int Id { get; set; }
        public string Title {  get; set; }
        public int Duration { get; set; }
        public int AlbumId { get; set; }
        public int BandId { get; set; }
    }
}
