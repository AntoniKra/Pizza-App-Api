using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PizzaApp.DTOs
{
    public class UpdatePizzaDto
    {

        [Required(ErrorMessage = "Nazwa jest wymagana")]
        [MaxLength(100)]
        public required string Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, 10000)]
        public decimal Price { get; set; }
        
        public string? ImageUrl { get; set; }
        
        // Nowy plik obrazka (opcjonalny)
        public IFormFile? ImageFile { get; set; }

        [Range(1, 10000)]
        public double WeightGrams { get; set; }
        public double Kcal { get; set; }

        [Required] 
        public required LookUpItemDto Style { get; set; }
        
        [Required] 
        public required LookUpItemDto BaseSauce { get; set; }
        
        [Required] 
        public required LookUpItemDto Dough { get; set; }
        
        [Required] 
        public required LookUpItemDto Thickness { get; set; }
        
        [Required] 
        public required LookUpItemDto Shape { get; set; }

        public double? DiameterCm { get; set; }
        public double? WidthCm { get; set; }
        public double? LengthCm { get; set; }

        // Nowa lista składników (zastępuje starą)
        public List<Guid> IngredientIds { get; set; } = new();
    }
}