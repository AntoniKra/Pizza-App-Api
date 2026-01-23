namespace PizzaApp.DTOs
{
    public class MenuDetailsDto : MenuListItemDto
    {
        public List<PizzaSearchResultDto> Pizzas { get; set; } = new();
    }
}
