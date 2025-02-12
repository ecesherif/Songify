using System.ComponentModel.DataAnnotations;

namespace Songify.Models.AlbumModels
{
    public class AlbumEditBindingModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Release Year must be a positive number.")]
        [Display(Name = "Release Year")]
        public int ReleaseYear { get; set; }
    }
}
