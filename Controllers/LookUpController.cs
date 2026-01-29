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

        // GET: api/LookUp/enum?type=PizzaStyleEnum&value=Neapolitan
        [HttpGet("enum")]
        public ActionResult<LookUpItemDto> GetEnumValue([FromQuery] string type, [FromQuery] string value)
        {
            if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(value))
            {
                return BadRequest("Typ i wartość enuma są wymagane.");
            }

            // Znajdź typ enuma w namespace PizzaApp.Enums
            var enumType = Type.GetType($"PizzaApp.Enums.{type}");
            
            if (enumType == null || !enumType.IsEnum)
            {
                return BadRequest($"Nieprawidłowy typ enuma: {type}");
            }

            // Spróbuj sparsować wartość
            if (!Enum.TryParse(enumType, value, true, out var enumValue))
            {
                return BadRequest($"Nieprawidłowa wartość dla enuma {type}: {value}");
            }

            // Użyj extension method do konwersji na LookUpItemDto
            var result = ((Enum)enumValue).ToLookUpItemDto();

            return Ok(result);
        }

        // GET: api/LookUp/enum/all?type=PizzaStyleEnum
        [HttpGet("enum/all")]
        public ActionResult<List<LookUpItemDto>> GetAllEnumValues([FromQuery] string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return BadRequest("Typ enuma jest wymagany.");
            }

            // Znajdź typ enuma w namespace PizzaApp.Enums
            var enumType = Type.GetType($"PizzaApp.Enums.{type}");
            
            if (enumType == null || !enumType.IsEnum)
            {
                return BadRequest($"Nieprawidłowy typ enuma: {type}");
            }

            // Pobierz wszystkie wartości i przekonwertuj na LookUpItemDto
            var values = Enum.GetValues(enumType)
                .Cast<Enum>()
                .Select(e => e.ToLookUpItemDto())
                .ToList();

            return Ok(values);
        }
    }
}
