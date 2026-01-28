using System.ComponentModel.DataAnnotations;
using PizzaApp.Enums;

namespace PizzaApp.DTOs
{
    public class PizzaSearchCriteriaDto
    {
        [Required(ErrorMessage = "Id miasta jest wymagane")]
        public required string CityId { get; set; }

        public List<Guid>? BrandIds { get; set; }
        public List<LookUpItemDto>? Styles { get; set; }
        public List<LookUpItemDto>? Doughs { get; set; }
        public List<LookUpItemDto>? Thicknesses { get; set; }
        public List<LookUpItemDto>? Shapes { get; set; }
        public List<LookUpItemDto>? Sauces { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public SortOptionEnum SortBy { get; set; } = SortOptionEnum.Default;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}