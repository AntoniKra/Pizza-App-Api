using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Enums;
using PizzaApp.Utils;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookUpController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LookUpController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/LookUp/filters
        [HttpGet("filters")]
        public async Task<ActionResult<PizzaFiltersDto>> GetFilters()
        {
            var brands = await _context.Brands
                .Select(b => new LookUpItemDto
                {
                    Id = b.Id.ToString(),
                    Name = b.Name
                })
                .ToListAsync();

            var filters = new PizzaFiltersDto
            {
                Restaurants = brands,
                Styles = EnumHelper.GetAllValues<PizzaStyleEnum>(),
                Doughs = EnumHelper.GetAllValues<DoughTypeEnum>(),
                Thicknesses = EnumHelper.GetAllValues<CrustThicknessEnum>(),
                Shapes = EnumHelper.GetAllValues<PizzaShapeEnum>(),
                Sauces = EnumHelper.GetAllValues<SauceTypeEnum>(),
                MaxPriceLimit = 150
            };

            return Ok(filters);
        }
    }
}
