using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Entities
{
    public class Brand
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? Logo { get; set; }

        public Guid OwnerId { get; set; }
        public Owner? Owner { get; set; } 
        public ICollection<Pizzeria> Pizzerias { get; set; } = new List<Pizzeria>();
    }
}
