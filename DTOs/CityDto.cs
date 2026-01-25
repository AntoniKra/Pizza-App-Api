namespace PizzaApp.DTOs
{
    public class CityDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Region { get; set; }
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }
    }
}
