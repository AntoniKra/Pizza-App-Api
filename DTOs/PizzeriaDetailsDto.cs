namespace PizzaApp.DTOs
{
    public class PizzeriaDetailsDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string BrandName { get; set; }
        public required string PhoneNumber { get; set; }

        public decimal DeliveryCost { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal ServiceFee { get; set; }
        public int AveragePreparationTimeMinutes { get; set; }
        public bool IsOpen { get; set; }

        public Guid? ActiveMenuId { get; set; }

        public required PizzeriaAddressDto Address { get; set; }
    }

    public class PizzeriaAddressDto
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
    }
}