namespace PizzaApp.DTOs
{
    public class PizzaFiltersDto
    {
        public List<LookUpItemDto> Restaurants { get; set; } = new List<LookUpItemDto>();
        public List<LookUpItemDto> Styles { get; set; } = new List<LookUpItemDto>();
        public List<LookUpItemDto> Doughs { get; set; } = new List<LookUpItemDto>();
        public List<LookUpItemDto> Thicknesses { get; set; } = new List<LookUpItemDto>();
        public List<LookUpItemDto> Shapes { get; set; } = new List<LookUpItemDto>();
        public List<LookUpItemDto> Sauces { get; set; } = new List<LookUpItemDto>();
        public decimal MaxPriceLimit { get; set; } = 150;
    }
}
