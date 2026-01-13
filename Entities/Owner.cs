namespace PizzaApp.Entities
{
    public class Owner : Account
    {
        public required string TaxId { get; set; }
        public ICollection<Brand> Brands { get; set; } = new List<Brand>();
    }
}
