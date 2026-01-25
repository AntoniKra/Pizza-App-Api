using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class UpdateCityDto
    {
        [Required(ErrorMessage = "Nazwa miasta jest wymagana")]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Region jest wymagany")]
        [MaxLength(100)]
        public required string Region { get; set; }

        [Required(ErrorMessage = "ID kraju jest wymagane")]
        public required Guid CountryId { get; set; }
    }
}
