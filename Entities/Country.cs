using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Entities
{
    public class Country
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required string IsoCode { get; set; }
        public required string PhonePrefix { get; set; }
    }
}
