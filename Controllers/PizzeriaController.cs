using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Entities;
using PizzaApp.Services;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PizzeriaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserContextService _userContextService;

        public PizzeriaController(AppDbContext context,UserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        // POST: api/Pizzeria
        [HttpPost]
        public async Task<ActionResult> CreatePizzeria([FromBody] CreatePizzeriaDto dto)
        {
            var brand = await _context.Brands.FindAsync(dto.Brand.Id);
            if (brand == null)
                return BadRequest("Podana marka nie istnieje.");
            var userId =  _userContextService.GetUserId();

            if (brand.Owner.Id != userId) return Forbid();

            var city = await _context.Cities.FirstOrDefaultAsync(c => c.Name == dto.Address.CityName);
            if (city == null)
            {
                city = new City
                {
                    Name = dto.Address.CityName,
                    Region = dto.Address.Region
                };
            }

            var address = new Address
            {
                Street = dto.Address.Street,
                BuildingNumber = dto.Address.BuildingNumber,
                ApartmentNumber = dto.Address.ApartmentNumber,
                ZipCode = dto.Address.ZipCode,
                City = city
            };

            var menu = new Menu
            {
                Name = $"Menu {dto.Name}",
                IsActive = true
            };

            var pizzeria = new Pizzeria
            {
                BrandId = brand.Id,
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber,
                DeliveryCost = dto.DeliveryCost,
                MinOrderAmount = dto.MinOrderAmount,
                ServiceFee = dto.ServiceFee,
                AveragePreparationTimeMinutes = dto.AveragePreparationTimeMinutes,
                MaxDeliveryRange = dto.MaxDeliveryRange,

                Address = address,

                Menus = new List<Menu> { menu }
            };

            _context.Pizzerias.Add(pizzeria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPizzeria), new { id = pizzeria.Id }, new { id = pizzeria.Id, name = pizzeria.Name });
        }

        // GET: api/Pizzeria/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PizzeriaDetailsDto>> GetPizzeria(Guid id)
        {
            var pizzeria = await _context.Pizzerias
                .Include(p => p.Brand)
                .Include(p => p.Menus)
                .Include(p => p.Address)
                    .ThenInclude(a => a.City)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pizzeria == null)
                return NotFound("Pizzeria nie istnieje.");

            var activeMenuId = pizzeria.Menus.FirstOrDefault(m => m.IsActive)?.Id;

            var dto = new PizzeriaDetailsDto
            {
                Id = pizzeria.Id,
                Name = pizzeria.Name,
                BrandName = pizzeria.Brand?.Name ?? "Nieznana marka",
                PhoneNumber = pizzeria.PhoneNumber,

                DeliveryCost = pizzeria.DeliveryCost,
                MinOrderAmount = pizzeria.MinOrderAmount,
                ServiceFee = pizzeria.ServiceFee,
                AveragePreparationTimeMinutes = pizzeria.AveragePreparationTimeMinutes,
                IsOpen = true,

                ActiveMenuId = activeMenuId,

                Address = new PizzeriaAddressDto
                {
                    Street = pizzeria.Address.Street,
                    City = pizzeria.Address.City?.Name ?? "Nieznane",
                    FullAddress = $"{pizzeria.Address.Street} {pizzeria.Address.BuildingNumber}" +
                                  (string.IsNullOrEmpty(pizzeria.Address.ApartmentNumber) ? "" : $"/{pizzeria.Address.ApartmentNumber}") +
                                  $", {pizzeria.Address.ZipCode} {pizzeria.Address.City?.Name}"
                }
            };

            return Ok(dto);
        }
    }
}