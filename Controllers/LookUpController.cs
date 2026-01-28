using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Enums;
using PizzaApp.Utils;
using PizzaApp.Extensions;

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

            // Pobranie Enumów, zeby Front dostał ID(string) i Name(opis po polsku), np. ID="Neopolitan", Name="Neapolitańska"
            var filters = new PizzaFiltersDto
            {
                Restaurants = brands,
                Styles = Enum.GetValues<PizzaStyleEnum>().Select(e => e.ToLookUpItemDto()).ToList(),
                Doughs = Enum.GetValues<DoughTypeEnum>().Select(e => e.ToLookUpItemDto()).ToList(),
                Thicknesses = Enum.GetValues<CrustThicknessEnum>().Select(e => e.ToLookUpItemDto()).ToList(),
                Shapes = Enum.GetValues<PizzaShapeEnum>().Select(e => e.ToLookUpItemDto()).ToList(),
                Sauces = Enum.GetValues<SauceTypeEnum>().Select(e => e.ToLookUpItemDto()).ToList(),
                MaxPriceLimit = 150
            };

            return Ok(filters);
        }
    }
}
