using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class CreateAddressDto
    {
        [Required]
        public required string Street { get; set; }

        [Required]
        public required string BuildingNumber { get; set; }

        public string? ApartmentNumber { get; set; }

        [Required]
        public required string ZipCode { get; set; }
        [Required]
        public Guid CityId { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
