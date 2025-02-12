using System.ComponentModel.DataAnnotations;

namespace Songify.Models.LikedSongsModels
{
    public class LikedSongAddBindingModel
    {
        [Required]
        [Display(Name = "UserId")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "SongId")]
        public int SongId { get; set; }
    }
}
