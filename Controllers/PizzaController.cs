using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Entities;
using PizzaApp.Enums;
using PizzaApp.Extensions;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PizzaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PizzaController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult> CreatePizza([FromBody] CreatePizzaDto dto)
        {
            // Validation
            var menuExists = await _context.Menus.AnyAsync(m => m.Id == dto.MenuId);
            if (!menuExists)
            {
                return BadRequest("Wybrane menu nie istnieje lub jest nieprawidłowe.");
            }

            if (await _context.Pizzas.AnyAsync(p => p.MenuId == dto.MenuId && p.Name == dto.Name))
            {
                return Conflict($"Pizza '{dto.Name}' już istnieje w tym menu.");
            }

            if (dto.Shape == PizzaShapeEnum.Round)
            {
                if (dto.DiameterCm == null || dto.DiameterCm <= 0)
                    return BadRequest("Dla pizzy okrągłej wymagana jest średnica.");
                dto.WidthCm = null;
                dto.LengthCm = null;
            }
            else if (dto.Shape == PizzaShapeEnum.Rectangle)
            {
                if (dto.WidthCm == null || dto.LengthCm == null)
                    return BadRequest("Dla pizzy prostokątnej wymagane są wymiary boków.");
                dto.DiameterCm = null;
            }

            var existingIngredients = await _context.Ingredients
                .Where(i => dto.IngredientIds.Contains(i.Id))
                .ToListAsync();

            if (existingIngredients.Count != dto.IngredientIds.Count)
            {
                return BadRequest("Podano nieistniejące składniki.");
            }

            // MAPPING (DTO -> Entity)
            var pizza = new Pizza
            {
                MenuId = dto.MenuId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                WeightGrams = dto.WeightGrams,
                Kcal = dto.Kcal,

                Style = dto.Style,
                Dough = dto.Dough,
                BaseSauce = dto.BaseSauce,
                Thickness = dto.Thickness,
                Shape = dto.Shape,

                DiameterCm = dto.DiameterCm ?? 0,
                WidthCm = dto.WidthCm ?? 0,
                LengthCm = dto.LengthCm ?? 0,

                Ingredients = existingIngredients
            };

            _context.Pizzas.Add(pizza);
            await _context.SaveChangesAsync();

            return Created($"api/pizza/{pizza.Id}", new { id = pizza.Id, name = pizza.Name });
        }

        // Zwraca tylko to, co potrzebne do wyświetlenia menu
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PizzaSearchResultDto>>> GetPizzas()
        {
            var pizzas = await _context.Pizzas
                .Select(p => new PizzaSearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    IngredientNames = p.Ingredients.Select(i => i.Name).ToList()
                })
                .ToListAsync();

            return Ok(pizzas);
        }

        // Używane, gdy klient kliknie w konkretną pizzę
        [HttpGet("{id}")]
        public async Task<ActionResult<PizzaDetailsDto>> GetPizza(Guid id)
        {
            var pizza = await _context.Pizzas
                .Include(p => p.Ingredients)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pizza == null)
            {
                return NotFound($"Wybrana nie istnieje.");
            }

            var detailsDto = new PizzaDetailsDto
            {
                Id = pizza.Id,
                Name = pizza.Name,

                Description = pizza.Description,

                ImageUrl = pizza.ImageUrl,

                Price = pizza.Price,
                WeightGrams = pizza.WeightGrams,
                Kcal = pizza.Kcal,

                Style = pizza.Style.ToLookUpItemDto(),
                Dough = pizza.Dough.ToLookUpItemDto(),
                BaseSauce = pizza.BaseSauce.ToLookUpItemDto(),
                Thickness = pizza.Thickness.ToLookUpItemDto(),
                Shape = pizza.Shape.ToLookUpItemDto(),

                DiameterCm = pizza.DiameterCm,
                WidthCm = pizza.WidthCm,
                LengthCm = pizza.LengthCm,

                Ingredients = pizza.Ingredients.Select(i => new IngredientDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    IsAllergen = i.IsAllergen,
                    IsMeat = i.IsMeat
                }).ToList()
            };

            return Ok(detailsDto);
        }
    }
}