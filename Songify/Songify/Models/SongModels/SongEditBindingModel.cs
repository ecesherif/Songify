using System.ComponentModel.DataAnnotations;
namespace Songify.Models.SongModels
{
    public class SongEditBindingModel
    {
        [Required]
        public int Id { get; set; } 
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Duration must be a positive number.")]
        [Display(Name = "Duration (seconds)")]
        public int Duration { get; set; }
        [Required]
        [Display(Name = "Album")]
        public int AlbumId { get; set; }
        [Required]
        [Display(Name = "Band")]
        public int BandId { get; set; }
    }
}

