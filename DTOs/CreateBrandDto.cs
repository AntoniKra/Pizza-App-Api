using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class CreateBrandDto
    {
        [Required]
        public required string Name { get; set; }

        public string? Logo { get; set; }

    }
}
