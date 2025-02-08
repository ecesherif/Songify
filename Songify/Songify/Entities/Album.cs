namespace Songify.Entities
{
    public class Album
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }
        public ICollection<Song> Songs { get; set; } = new List<Song>();

    }
}
