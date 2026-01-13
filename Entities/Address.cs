using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Entities
{
    public class Address
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Street { get; set; }
        public required string BuildingNumber { get; set; }
        public string? ApartmentNumber { get; set; }
        public required string ZipCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid CityId { get; set; }
        public City? City { get; set; }
    }
}
