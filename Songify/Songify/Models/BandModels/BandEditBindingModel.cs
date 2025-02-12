using System.ComponentModel.DataAnnotations;

namespace Songify.Models.BandModels
{
    public class BandEditBindingModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Form Year must be a positive number.")]
        [Display(Name = "Form Year")]
        public int FormYear { get; set; }
        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }
    }
}
