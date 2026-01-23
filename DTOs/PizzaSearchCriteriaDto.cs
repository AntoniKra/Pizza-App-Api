using System.ComponentModel.DataAnnotations;
using PizzaApp.Enums;

namespace PizzaApp.DTOs
{
    public class PizzaSearchCriteriaDto
    {
        [Required(ErrorMessage = "Id miasta jest wymagane")]
        public required string CityId { get; set; }

        public List<Guid>? BrandIds { get; set; }
        public List<PizzaStyleEnum>? Styles { get; set; }
        public List<DoughTypeEnum>? Doughs { get; set; }
        public List<CrustThicknessEnum>? Thicknesses { get; set; }
        public List<PizzaShapeEnum>? Shapes { get; set; }
        public List<SauceTypeEnum>? Sauces { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public SortOptionEnum SortBy { get; set; } = SortOptionEnum.Default;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}