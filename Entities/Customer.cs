namespace PizzaApp.Entities
{
    public class Customer : Account
    {
        public Guid? AddressId { get; set; }
        public Address? Address { get; set; }
    }
}
