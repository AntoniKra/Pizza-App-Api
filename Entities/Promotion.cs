using PizzaApp.Enums;
using System.Text.Json.Serialization;

namespace PizzaApp.Entities
{
    public class Promotion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal DiscountValue { get; set; }
        public PromotionTypeEnum Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
        public Guid PizzeriaId { get; set; }
        [JsonIgnore]
        public Pizzeria Pizzeria { get; set; } = null!;
        public ICollection<Pizza> Pizzas { get; set; } = new List<Pizza>();
    }
}
