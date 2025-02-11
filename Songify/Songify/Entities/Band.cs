namespace Songify.Entities
{
    public class Band
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FormYear { get; set; }
        public string Country { get; set; }
        public ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}
