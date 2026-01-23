using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Entities;
using PizzaApp.Interfaces;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;

        public BrandController(AppDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        // GET: api/Brand/GetAll
        [HttpGet("GetAll")]
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
                Pizzerias = [.. brand.Pizzerias.Select(p => new PizzeriaSimpleDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    City = p.Address?.City?.Name ?? "Nieznane"
                })]
            };

            return Ok(details);
        }


        // POST: api/Brand/
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateBrand([FromBody] CreateBrandDto dto)
        {
            var userId = _userContextService.GetUserId();

            if(!userId.HasValue)
            {
                return Unauthorized();
            }
            var ownerExists = await _context.Owners.AnyAsync(o => o.Id == userId);

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
                OwnerId = userId.Value
            };

            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBrand), new { id = brand.Id }, new BrandDto { Id = brand.Id, Name = brand.Name });
        }

        // DELETE: api/Brand/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBrand(Guid id)
        {
            var userId = _userContextService.GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }
            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound("Marka nie istnieje.");
            }
            if(brand.Owner!.Id != userId.Value)
            {
                return Forbid();
            }

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}