namespace PizzaApp.DTOs
{
    public class PizzaSearchResultDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public double WeightGrams { get; set; }
        public double Kcal { get; set; }
        public double? DiameterCm { get; set; }
        public string StyleName { get; set; } = string.Empty;
        public decimal PricePerSqCm { get; set; }

        public List<string> IngredientNames { get; set; } = new();
    }
}