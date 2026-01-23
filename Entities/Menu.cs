using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Entities
{
    public class Menu
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public Guid PizzeriaId { get; set; }
        public Pizzeria Pizzeria { get; set; } = null!;
        public ICollection<Pizza> Pizzas { get; set; } = new List<Pizza>();
    }
}
