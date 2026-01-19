using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Entities;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BrandController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Brand
        [HttpPost]
        public async Task<ActionResult> CreateBrand([FromBody] CreateBrandDto dto)
        {
            var ownerExists = await _context.Owners.AnyAsync(o => o.Id == dto.OwnerId);
            if (!ownerExists)
            {
                return BadRequest("Podany właściciel nie istnieje.");
            }

            if (await _context.Brands.AnyAsync(b => b.Name == dto.Name))
            {
                return Conflict($"Marka o nazwie '{dto.Name}' już istnieje.");
            }

            var brand = new Brand
            {
                Name = dto.Name,
                Logo = dto.Logo,
                OwnerId = dto.OwnerId
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, new BrandDto { Id = brand.Id, Name = brand.Name });
        }

        // GET: api/Brand
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BrandDto>>> GetBrands()
        {
            var brands = await _context.Brands
                .Select(b => new BrandDto
                {
                    Id = b.Id,
                    Name = b.Name
                })
                .ToListAsync();

            return Ok(brands);
        }

        // GET: api/Brand/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BrandDetailsDto>> GetBrand(Guid id)
        {
            var brand = await _context.Brands
                .Include(b => b.Pizzerias)
                .ThenInclude(p => p.Address)
                    .ThenInclude(a => a.City)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (brand == null)
            {
                return NotFound("Wybrana marka nie istnieje.");
            }

            var details = new BrandDetailsDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Logo = brand.Logo,
                Pizzerias = brand.Pizzerias.Select(p => new PizzeriaSimpleDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    City = p.Address?.City?.Name ?? "Nieznane"
                }).ToList()
            };

            return Ok(details);
        }

        // DELETE: api/Brand/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(Guid id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound("Marka nie istnieje.");
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}