using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Entities
{
    public class Ingredient
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public bool IsAllergen { get; set; }
        public bool IsMeat { get; set; }

        public ICollection<Pizza> Pizzas { get; set; } = new List<Pizza>();
    }
}
