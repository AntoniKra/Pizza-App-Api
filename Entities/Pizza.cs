using System.ComponentModel.DataAnnotations;
using PizzaApp.Enums; 

namespace PizzaApp.Entities
{
    public class Pizza
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public PizzaStyleEnum Style { get; set; }
        public SauceTypeEnum BaseSauce { get; set; }
        public DoughTypeEnum Dough { get; set; }
        public CrustThicknessEnum Thickness { get; set; }
        public PizzaShapeEnum Shape { get; set; }

        public double DiameterCm { get; set; }
        public double WidthCm { get; set; }    
        public double LengthCm { get; set; }  
        public double WeightGrams { get; set; }
        public double Kcal { get; set; }

        public Guid MenuId { get; set; }
        public ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        public ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
    }
}