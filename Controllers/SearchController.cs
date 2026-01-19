using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PizzeriaListItemDto>>> Search(
            [FromQuery] string? city,
            [FromQuery] string? searchPhrase)
        {
            var query = _context.Pizzerias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityLower = city.ToLower();
                query = query.Where(p =>
                    p.Address != null &&
                    p.Address.City != null &&
                    p.Address.City.Name.ToLower().Contains(cityLower));
            }

            if (!string.IsNullOrWhiteSpace(searchPhrase))
            {
                var phraseLower = searchPhrase.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(phraseLower) ||
                    (p.Brand != null && p.Brand.Name.ToLower().Contains(phraseLower)));
            }

            query = query.OrderBy(p => p.Name);

            if (string.IsNullOrWhiteSpace(city) && string.IsNullOrWhiteSpace(searchPhrase))
            {
                query = query.Take(20);
            }

            var results = await query
                .Select(p => new PizzeriaListItemDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BrandName = p.Brand != null ? p.Brand.Name : "Lokalna Pizzeria",
                    LogoUrl = p.Brand != null ? p.Brand.Logo : null,

                    City = p.Address != null && p.Address.City != null ? p.Address.City.Name : "Nieznane",
                    Street = p.Address != null ? p.Address.Street : "Brak danych",

                    DeliveryCost = p.DeliveryCost,
                    AveragePreparationTimeMinutes = p.AveragePreparationTimeMinutes,
                    MinOrderAmount = p.MinOrderAmount,
                    IsOpen = true
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}