using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class IngredientDto
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }
        public bool IsAllergen { get; set; }
        public bool IsMeat { get; set; }
    }
}