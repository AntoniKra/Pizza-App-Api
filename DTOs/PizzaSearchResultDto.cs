namespace PizzaApp.DTOs
{
    public class PizzaSearchResultDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public List<string> IngredientNames { get; set; } = new();
    }
}