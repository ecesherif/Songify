namespace Songify.Entities
{
    public class Genre
    {
        public int Id { get; set; }
        public int Name { get; set; }
        public virtual ICollection<Song> Songs { get; set; } = new List<Song>();
    }
}
