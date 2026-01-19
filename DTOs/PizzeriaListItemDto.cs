namespace PizzaApp.DTOs
{
    public class PizzeriaListItemDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string BrandName { get; set; }
        public string? LogoUrl { get; set; }

        public required string City { get; set; }
        public required string Street { get; set; }

        public decimal DeliveryCost { get; set; }
        public int AveragePreparationTimeMinutes { get; set; }
        public decimal MinOrderAmount { get; set; }

        public bool IsOpen { get; set; } = true;
    }
}