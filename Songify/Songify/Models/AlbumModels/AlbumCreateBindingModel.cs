using System.ComponentModel.DataAnnotations;

namespace Songify.Models.AlbumModels
{
    public class AlbumCreateBindingModel
    {
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Release Year")]
        public int ReleaseYear { get; set; }
    }
}
