using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Entities;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CountryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Country/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<CountryDto>>> GetCountries()
        {
            var countries = await _context.Countries
                .Select(c => new CountryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsoCode = c.IsoCode,
                    PhonePrefix = c.PhonePrefix
                })
                .ToListAsync();

            return Ok(countries);
        }

        // GET: api/Country/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(Guid id)
        {
            var country = await _context.Countries.FindAsync(id);

            if (country == null)
            {
                return NotFound("Kraj nie istnieje.");
            }

            var countryDto = new CountryDto
            {
                Id = country.Id,
                Name = country.Name,
                IsoCode = country.IsoCode,
                PhonePrefix = country.PhonePrefix
            };

            return Ok(countryDto);
        }

        // POST: api/Country
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CountryDto>> CreateCountry(CreateCountryDto dto)
        {
            var countryExists = await _context.Countries
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() || c.IsoCode.ToLower() == dto.IsoCode.ToLower());
            if (countryExists)
            {
                return Conflict($"Kraj o nazwie '{dto.Name}' lub kodzie ISO '{dto.IsoCode}' ju¿ istnieje.");
            }

            var country = new Country
            {
                Name = dto.Name,
                IsoCode = dto.IsoCode.ToUpper(),
                PhonePrefix = dto.PhonePrefix
            };

            _context.Countries.Add(country);
            await _context.SaveChangesAsync();

            var resultDto = new CountryDto
            {
                Id = country.Id,
                Name = country.Name,
                IsoCode = country.IsoCode,
                PhonePrefix = country.PhonePrefix
            };

            return CreatedAtAction(nameof(GetCountry), new { id = resultDto.Id }, resultDto);
        }

        // PUT: api/Country/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCountry(Guid id, UpdateCountryDto dto)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound("Kraj nie istnieje.");
            }

            var duplicateExists = await _context.Countries
                .AnyAsync(c => c.Id != id && (c.Name.ToLower() == dto.Name.ToLower() || c.IsoCode.ToLower() == dto.IsoCode.ToLower()));
            if (duplicateExists)
            {
                return Conflict($"Kraj o nazwie '{dto.Name}' lub kodzie ISO '{dto.IsoCode}' ju¿ istnieje.");
            }

            country.Name = dto.Name;
            country.IsoCode = dto.IsoCode.ToUpper();
            country.PhonePrefix = dto.PhonePrefix;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Country/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCountry(Guid id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound("Kraj nie istnieje.");
            }

            var citiesUsingCountry = await _context.Cities.AnyAsync(c => c.CountryId == id);
            if (citiesUsingCountry)
            {
                return BadRequest("Nie mo¿na usun¹æ kraju, poniewa¿ s¹ z nim powi¹zane miasta.");
            }

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CountryExists(Guid id)
        {
            return _context.Countries.Any(e => e.Id == id);
        }
    }
}
