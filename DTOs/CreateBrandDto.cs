using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Entities
{
    public class CreateAddressDtoBrand
    {
        [Required]
        public required string Name { get; set; }

        public string? Logo { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        [Required]
        public Owner? Owner { get; set; }
        public ICollection<Pizzeria> Pizzerias { get; set; } = new List<Pizzeria>();
    }
}
