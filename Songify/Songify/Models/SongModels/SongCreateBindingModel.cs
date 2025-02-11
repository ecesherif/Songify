using System.ComponentModel.DataAnnotations;

namespace Songify.Models.SongModels
{
    public class SongCreateBindingModel
    {
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Duration must be a positive number.")]
        [Display(Name = "Duration in s")]
        public int Duration { get; set; }

        [Required]
        [ExistsInDatabase("Albums", "AlbumId")]
        [Display(Name = "Album Id")]
        public int AlbumId { get; set; }

        [Required]
        [ExistsInDatabase("Bands", "BandId")]
        [Display(Name = "Band Id")]
        public int BandId { get; set; }
    }
}
