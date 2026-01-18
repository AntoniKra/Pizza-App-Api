using PizzaApp.Enums;

namespace PizzaApp.DTOs
{
    public class PizzaDetailsDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public required string Style { get; set; }
        public required string Dough { get; set; }
        public required string BaseSauce { get; set; }
        public required string Thickness { get; set; }
        public required string Shape { get; set; }

        public double WeightGrams { get; set; }
        public double Kcal { get; set; }
        public double? DiameterCm { get; set; }
        public double? WidthCm { get; set; }
        public double? LengthCm { get; set; }

        public List<IngredientDto> Ingredients { get; set; } = new();
    }
}