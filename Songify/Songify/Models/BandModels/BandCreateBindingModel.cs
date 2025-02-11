using System.ComponentModel.DataAnnotations;

namespace Songify.Models.BandModels
{
    public class BandCreateBindingModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Form Year")]
        public int FormYear { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }
    }
}
