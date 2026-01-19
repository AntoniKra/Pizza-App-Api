using PizzaApp.Enums;

namespace PizzaApp.DTOs
{
    public class PizzaDetailsDto
    {
        public Guid Id { get; set; }
        public Guid MenuId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public required LookUpItemDto Style { get; set; }
        public required LookUpItemDto Dough { get; set; }
        public required LookUpItemDto BaseSauce { get; set; }
        public required LookUpItemDto Thickness { get; set; }
        public required LookUpItemDto Shape { get; set; }
        public double WeightGrams { get; set; }
        public double Kcal { get; set; }
        public double? DiameterCm { get; set; }
        public double? WidthCm { get; set; }
        public double? LengthCm { get; set; }

        public List<IngredientDto> Ingredients { get; set; } = new();
    }
}