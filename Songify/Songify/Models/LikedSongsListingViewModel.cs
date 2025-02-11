using System.ComponentModel.DataAnnotations.Schema;

namespace Songify.Models
{
    public class LikedSongsListingViewModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string SongId { get; set; }
    }
}
