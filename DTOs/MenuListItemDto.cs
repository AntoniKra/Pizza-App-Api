namespace PizzaApp.DTOs
{
    public class MenuListItemDto
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int PizzasCount { get; set; } 
    }
}
