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
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AddressController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Address/GetAdress/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Address>> GetAddress(Guid id)
        {
            var address = await _context.Addresses
                .Include(a => a.City)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (address == null)
            {
                return NotFound();
            }

            return address;
        }

        // POST: api/Address/Create
        [HttpPost]
        public async Task<ActionResult<Address>> CreateAddress(CreateAddressDto dto)
        {
            // Opcjonalnie: Sprawdź czy miasto istnieje
            var cityExists = await _context.Cities.AnyAsync(c => c.Id == dto.CityId);
            if (!cityExists)
            {
                return BadRequest($"Miasto o ID {dto.CityId} nie istnieje.");
            }

            var address = new Address
            {
                Street = dto.Street,
                BuildingNumber = dto.BuildingNumber,
                ApartmentNumber = dto.ApartmentNumber,
                ZipCode = dto.ZipCode,
                CityId = dto.CityId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAddress), new { id = address.Id }, address);
        }

        // PUT: api/Address/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, UpdateAddressDto dto)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            address.Street = dto.Street;
            address.BuildingNumber = dto.BuildingNumber;
            address.ApartmentNumber = dto.ApartmentNumber;
            address.ZipCode = dto.ZipCode;
            address.CityId = dto.CityId;
            address.Latitude = dto.Latitude;
            address.Longitude = dto.Longitude;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AddressExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/Address/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
            {
                return NotFound();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // GET: api/Address/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<Address>>> GetAddresses()
        {
            return await _context.Addresses
                .Include(a => a.City)
                .ThenInclude(c => c.Country)
                .ToListAsync();
        }

        private bool AddressExists(Guid id)
        {
            return _context.Addresses.Any(e => e.Id == id);
        }
    }
}