using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class CreateIngredientDto
    {
        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [MaxLength(50, ErrorMessage = "Nazwa składnika jest za długa")]
        [MinLength(2, ErrorMessage = "Nazwa musi mieć co najmniej 2 znaki")]
        public required string Name { get; set; }
        public bool IsAllergen { get; set; }
        public bool IsMeat { get; set; }
    }
}