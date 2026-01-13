using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Entities
{
    public class Pizzeria
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public decimal MinOrderAmount { get; set; }
        public decimal DeliveryCost { get; set; }
        public required string PhoneNumber { get; set; }
        public decimal ServiceFee { get; set; }
        public int AveragePreparationTimeMinutes { get; set; }
        public double MaxDeliveryRange { get; set; }

        public Guid BrandId { get; set; }
        public Brand? Brand { get; set; } 

        public Guid AddressId { get; set; }
        public Address? Address { get; set; } 

        public ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();
        public ICollection<Menu> Menus { get; set; } = new List<Menu>();
        public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    }
}
