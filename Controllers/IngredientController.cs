using Microsoft.AspNetCore.Authorization;
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

    public class IngredientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IngredientController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ingredients/${id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<IngredientDto>>> GetIngredient(Guid id)
        {
            var ingredients = await _context.Ingredients.SingleOrDefaultAsync(x => x.Id == id);
            if (ingredients == null)
            {
                return NotFound();
            }

            return Ok(ingredients);
        }

        // POST: api/ingredients
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<IngredientDto>> CreateIngredient(CreateIngredientDto dto)
        {

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Nazwa składnika nie może być pusta.");
            }

            var trimmed = dto.Name.Trim();
            var normalizedName = char.ToUpper(trimmed[0]) + trimmed.Substring(1).ToLower();

            var exists = await _context.Ingredients
                .AnyAsync(i => i.Name.ToLower() == normalizedName.ToLower());

            if (exists)
            {
                return Conflict($"Składnik '{normalizedName}' już istnieje.");
            }

            var ingredient = new Ingredient
            {
                Name = normalizedName,
                IsAllergen = dto.IsAllergen,
                IsMeat = dto.IsMeat,
            };

            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            var resultDto = new IngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                IsAllergen = ingredient.IsAllergen,
                IsMeat = ingredient.IsMeat,
            };

            return CreatedAtAction(nameof(CreateIngredient), new { id = resultDto.Id }, resultDto);
        }

        // GET: api/ingredients/GetAll
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<IngredientDto>>> GetIngredients()
        {
            var ingredients = await _context.Ingredients
                .Select(i => new IngredientDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    IsAllergen = i.IsAllergen,
                    IsMeat = i.IsMeat,
                })
                .ToListAsync();

            return Ok(ingredients);
        }

      


    }
}