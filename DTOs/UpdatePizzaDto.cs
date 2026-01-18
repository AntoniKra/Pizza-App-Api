using System.ComponentModel.DataAnnotations;
using PizzaApp.Enums;

namespace PizzaApp.DTOs
{
    public class UpdatePizzaDto
    {
        [Required]
        public Guid MenuId { get; set; }

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [MaxLength(100)]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, 10000)]
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        [Range(1, 10000)]
        public double WeightGrams { get; set; }
        public double Kcal { get; set; }

        [Required] public PizzaStyleEnum Style { get; set; }
        [Required] public SauceTypeEnum BaseSauce { get; set; }
        [Required] public DoughTypeEnum Dough { get; set; }
        [Required] public CrustThicknessEnum Thickness { get; set; }
        [Required] public PizzaShapeEnum Shape { get; set; }

        public double? DiameterCm { get; set; }
        public double? WidthCm { get; set; }
        public double? LengthCm { get; set; }

        // Nowa lista składników (zastępuje starą)
        public List<Guid> IngredientIds { get; set; } = new();
    }
}