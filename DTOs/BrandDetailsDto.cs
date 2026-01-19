namespace PizzaApp.DTOs
{
    public class BrandDetailsDto : BrandDto
    {
        public string? Logo { get; set; }
        public List<PizzeriaSimpleDto> Pizzerias { get; set; } = new();
    }

    public class PizzeriaSimpleDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? City { get; set; }
    }
}