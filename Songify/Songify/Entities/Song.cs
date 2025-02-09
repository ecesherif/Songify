namespace Songify.Entities
{
    public class Song
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public int AlbumId { get; set; }
        public Album Album { get; set; }
        public int BandId { get; set; }
        public Band Band { get; set; }
    }
}
