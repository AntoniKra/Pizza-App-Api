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
            // Start zapytania z Eager Loading
            var query = _context.Pizzerias
                .Include(p => p.Brand)
                .Include(p => p.Address)
                    .ThenInclude(a => a.City)
                .AsQueryable();

            // Filtrowanie po Mieście (jeśli podano)
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(p =>
                    p.Address != null &&
                    p.Address.City != null &&
                    p.Address.City.Name.Contains(city));
            }

            // Filtrowanie po Nazwie (Pizzerii lub Marki)
            if (!string.IsNullOrWhiteSpace(searchPhrase))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchPhrase) ||
                    (p.Brand != null && p.Brand.Name.Contains(searchPhrase)));
            }

            // Logika "Best Match" dla pustych filtrów
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