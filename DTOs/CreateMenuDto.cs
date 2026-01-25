using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class CreateMenuDto
    {
        [Required]
        public required LookUpItemDto Pizzeria { get; set; }

        [Required(ErrorMessage = "Nazwa menu jest wymagana")]
        [MaxLength(50)]
        public required string Name { get; set; }

        public string? Description { get; set; }
    }
}