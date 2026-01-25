namespace PizzaApp.DTOs
{
    public class CountryDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string IsoCode { get; set; }
        public required string PhonePrefix { get; set; }
    }
}
