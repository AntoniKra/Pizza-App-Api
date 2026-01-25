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
    public class CityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CityController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/City/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CityDto>> GetCity(Guid id)
        {
            var city = await _context.Cities
                .Include(c => c.Country)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (city == null)
            {
                return NotFound("Miasto nie istnieje.");
            }

            var cityDto = new CityDto
            {
                Id = city.Id,
                Name = city.Name,
                Region = city.Region,
                CountryId = city.CountryId,
                CountryName = city.Country?.Name
            };

            return Ok(cityDto);
        }

        // POST: api/City
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CityDto>> CreateCity(CreateCityDto dto)
        {
            var countryExists = await _context.Countries.AnyAsync(c => c.Id == dto.CountryId);
            if (!countryExists)
            {
                return BadRequest($"Kraj o ID {dto.CountryId} nie istnieje.");
            }

            var cityExists = await _context.Cities
                .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.CountryId == dto.CountryId);
            if (cityExists)
            {
                return Conflict($"Miasto '{dto.Name}' ju¿ istnieje w tym kraju.");
            }

            var city = new City
            {
                Name = dto.Name,
                Region = dto.Region,
                CountryId = dto.CountryId
            };

            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            var country = await _context.Countries.FindAsync(dto.CountryId);

            var resultDto = new CityDto
            {
                Id = city.Id,
                Name = city.Name,
                Region = city.Region,
                CountryId = city.CountryId,
                CountryName = country?.Name
            };

            return CreatedAtAction(nameof(GetCity), new { id = resultDto.Id }, resultDto);
        }

        // PUT: api/City/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCity(Guid id, UpdateCityDto dto)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound("Miasto nie istnieje.");
            }

            var countryExists = await _context.Countries.AnyAsync(c => c.Id == dto.CountryId);
            if (!countryExists)
            {
                return BadRequest($"Kraj o ID {dto.CountryId} nie istnieje.");
            }

            var duplicateExists = await _context.Cities
                .AnyAsync(c => c.Id != id && c.Name.ToLower() == dto.Name.ToLower() && c.CountryId == dto.CountryId);
            if (duplicateExists)
            {
                return Conflict($"Miasto '{dto.Name}' ju¿ istnieje w tym kraju.");
            }

            city.Name = dto.Name;
            city.Region = dto.Region;
            city.CountryId = dto.CountryId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityExists(id))
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

        // DELETE: api/City/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCity(Guid id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound("Miasto nie istnieje.");
            }

            var addressesUsingCity = await _context.Addresses.AnyAsync(a => a.CityId == id);
            if (addressesUsingCity)
            {
                return BadRequest("Nie mo¿na usun¹æ miasta, poniewa¿ jest u¿ywane w adresach.");
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/City/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCities()
        {
            var cities = await _context.Cities
                .Include(c => c.Country)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Region = c.Region,
                    CountryId = c.CountryId,
                    CountryName = c.Country!.Name
                })
                .ToListAsync();

            return Ok(cities);
        }

       

        // GET: api/City/GetByCountry/{countryId}
        [HttpGet("GetByCountry/{countryId}")]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetCitiesByCountry(Guid countryId)
        {
            var cities = await _context.Cities
                .Where(c => c.CountryId == countryId)
                .Include(c => c.Country)
                .Select(c => new CityDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Region = c.Region,
                    CountryId = c.CountryId,
                    CountryName = c.Country!.Name
                })
                .ToListAsync();

            return Ok(cities);
        }

       

        private bool CityExists(Guid id)
        {
            return _context.Cities.Any(e => e.Id == id);
        }
    }
}
