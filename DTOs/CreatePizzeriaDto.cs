using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class CreatePizzeriaDto
    {
        [Required]
        public Guid BrandId { get; set; }

        [Required(ErrorMessage = "Nazwa pizzerii jest wymagana")]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        [Phone]
        public required string PhoneNumber { get; set; }

        [Range(0, 1000)]
        public decimal DeliveryCost { get; set; }

        [Range(0, 10000)]
        public decimal MinOrderAmount { get; set; }

        [Range(0, 100)]
        public decimal ServiceFee { get; set; }

        [Range(1, 120)]
        public int AveragePreparationTimeMinutes { get; set; } = 30;

        [Range(0, 100)]
        public double MaxDeliveryRange { get; set; } = 10.0;

        [Required]
        public required PizzeriaAddressRequest Address { get; set; }
    }
    public class PizzeriaAddressRequest
    {
        [Required]
        public required string Street { get; set; }

        [Required]
        public required string BuildingNumber { get; set; }

        public string? ApartmentNumber { get; set; }

        [Required]
        public required string ZipCode { get; set; }

        [Required]
        public required string CityName { get; set; }

        [Required]
        public required string Region { get; set; }
    }
}