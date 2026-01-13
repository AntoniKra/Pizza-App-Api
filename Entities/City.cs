using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Entities
{
    public class City
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public required string Region { get; set; }

        public Guid CountryId { get; set; }
        public Country? Country { get; set; }
    }
}
