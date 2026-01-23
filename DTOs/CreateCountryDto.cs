using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class CreateCountryDto
    {
        [Required(ErrorMessage = "Nazwa kraju jest wymagana")]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Kod ISO jest wymagany")]
        [MaxLength(3)]
        public required string IsoCode { get; set; }

        [Required(ErrorMessage = "Prefiks telefoniczny jest wymagany")]
        [MaxLength(5)]
        public required string PhonePrefix { get; set; }
    }
}
