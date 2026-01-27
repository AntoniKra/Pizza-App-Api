using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Entities;
using PizzaApp.Enums;
using PizzaApp.Extensions;
using PizzaApp.Services;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PizzaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;

        public PizzaController(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [HttpPost]
        public async Task<ActionResult> CreatePizza([FromForm] CreatePizzaDto dto)
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

            string? uploadedImageUrl = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                // Wysłanie do Google Cloud i zwrócenie linku
                uploadedImageUrl = await _fileService.UploadFileAsync(dto.ImageFile, "menu-items");
            }

            // MAPPING (DTO -> Entity)
            var pizza = new Pizza
            {
                MenuId = dto.MenuId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                ImageUrl = uploadedImageUrl,
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

            return CreatedAtAction(nameof(GetPizza), new { id = pizza.Id }, new
            {
                id = pizza.Id,
                name = pizza.Name,
                imageUrl = pizza.ImageUrl
            });
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<PizzaSearchResultDto>>> GetPizzas()
        {
            var pizzaData = await _context.Pizzas
                .Include(p => p.Ingredients)
                .Include(p => p.Menu)
                    .ThenInclude(m => m.Pizzeria)
                        .ThenInclude(pz => pz.Brand)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    BrandName = p.Menu.Pizzeria.Brand!.Name,
                    p.Description,
                    p.Price,
                    p.ImageUrl,
                    p.WeightGrams,
                    p.Kcal,
                    p.DiameterCm,
                    p.WidthCm,
                    p.LengthCm,
                    p.Shape,
                    p.Style,
                    IngredientNames = p.Ingredients.Select(i => i.Name).ToList()
                })
                .ToListAsync();

            var pizzas = pizzaData.Select(p =>
            {
                double area;
                if (p.Shape == PizzaShapeEnum.Round)
                {
                    var radius = p.DiameterCm / 2.0;
                    area = Math.PI * radius * radius;
                }
                else
                {
                    area = p.WidthCm * p.LengthCm;
                }

                var pricePerSqCm = area > 0 ? (decimal)p.Price / (decimal)area : 0m;

                return new PizzaSearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BrandName = p.BrandName,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    WeightGrams = p.WeightGrams,
                    Kcal = p.Kcal,
                    DiameterCm = p.Shape == PizzaShapeEnum.Round ? p.DiameterCm : null,
                    StyleName = p.Style.GetDescription(),
                    PricePerSqCm = Math.Round(pricePerSqCm, 4),
                    IngredientNames = p.IngredientNames
                };
            }).ToList();

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
                MenuId = pizza.MenuId,
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

        // PUT: api/Pizza/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePizza(Guid id, [FromBody] UpdatePizzaDto dto)
        {
            // POBIERANIE Z BAZY (EAGER LOADING)
            var pizza = await _context.Pizzas
                .Include(p => p.Ingredients)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pizza == null)
            {
                return NotFound("Wybrana pizza nie istnieje.");
            }

            if (!await _context.Menus.AnyAsync(m => m.Id == dto.MenuId))
            {
                return BadRequest("Wybrane menu nie istnieje.");
            }

            var nameTaken = await _context.Pizzas
                .AnyAsync(p => p.MenuId == dto.MenuId && p.Name == dto.Name && p.Id != id);

            if (nameTaken)
            {
                return Conflict("Inna pizza w tym menu ma już taką nazwę.");
            }

            if (dto.Shape == PizzaShapeEnum.Round && (dto.DiameterCm == null || dto.DiameterCm <= 0))
                return BadRequest("Dla pizzy okrągłej wymagana jest średnica.");

            if (dto.Shape == PizzaShapeEnum.Rectangle && (dto.WidthCm == null || dto.LengthCm == null))
                return BadRequest("Dla pizzy prostokątnej wymagane są wymiary boków.");

            // Pobieramy z bazy te składniki, których ID przesłał user
            var newIngredients = await _context.Ingredients
                .Where(i => dto.IngredientIds.Contains(i.Id))
                .ToListAsync();

            // Zabezpieczenie, gdy choć jeden z kilku ID nie istnieje
            if (newIngredients.Count != dto.IngredientIds.Count)
            {
                return BadRequest("Jeden lub więcej podanych składników nie istnieje.");
            }

            pizza.MenuId = dto.MenuId;
            pizza.Name = dto.Name;
            pizza.Description = dto.Description;
            pizza.ImageUrl = dto.ImageUrl;
            pizza.Price = dto.Price;
            pizza.WeightGrams = dto.WeightGrams;
            pizza.Kcal = dto.Kcal;

            pizza.Style = dto.Style;
            pizza.Dough = dto.Dough;
            pizza.BaseSauce = dto.BaseSauce;
            pizza.Thickness = dto.Thickness;
            pizza.Shape = dto.Shape;

            if (dto.Shape == PizzaShapeEnum.Round)
            {
                pizza.DiameterCm = dto.DiameterCm!.Value;
                pizza.WidthCm = 0;
                pizza.LengthCm = 0;
            }
            else
            {
                pizza.DiameterCm = 0;
                pizza.WidthCm = dto.WidthCm!.Value;
                pizza.LengthCm = dto.LengthCm!.Value;
            }

            // UPDATE RELACJI MANY-TO-MANY
            pizza.Ingredients = newIngredients;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Pizza/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePizza(Guid id)
        {
            var pizza = await _context.Pizzas.FindAsync(id);

            if (pizza == null)
            {
                return NotFound("Wybrana pizza nie istnieje.");
            }

            _context.Pizzas.Remove(pizza);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Pizza/search
        [HttpGet("search")]
        public async Task<ActionResult<List<PizzaSearchResultDto>>> SearchPizzas([FromQuery] PizzaSearchCriteriaDto criteria)
        {
            var query = _context.Pizzas
                .Include(p => p.Ingredients)
                .Include(p => p.Menu)
                    .ThenInclude(m => m.Pizzeria)
                        .ThenInclude(pz => pz.Address)
                            .ThenInclude(a => a.City)
                .Include(p => p.Menu)
                    .ThenInclude(m => m.Pizzeria)
                        .ThenInclude(pz => pz.Brand)
                .AsQueryable();

            // City Filter (Required)
            query = query.Where(p => p.Menu.Pizzeria.Address!.City!.Id.ToString() == criteria.CityId);

            // Only Active Menus
            query = query.Where(p => p.Menu.IsActive);

            // Brand Filter
            if (criteria.BrandIds != null && criteria.BrandIds.Any())
            {
                query = query.Where(p => criteria.BrandIds.Contains(p.Menu.Pizzeria.BrandId));
            }

            // Style Filter
            if (criteria.Styles != null && criteria.Styles.Any())
            {
                query = query.Where(p => criteria.Styles.Contains(p.Style));
            }

            // Dough Filter
            if (criteria.Doughs != null && criteria.Doughs.Any())
            {
                query = query.Where(p => criteria.Doughs.Contains(p.Dough));
            }

            // Thickness Filter
            if (criteria.Thicknesses != null && criteria.Thicknesses.Any())
            {
                query = query.Where(p => criteria.Thicknesses.Contains(p.Thickness));
            }

            // Shape Filter
            if (criteria.Shapes != null && criteria.Shapes.Any())
            {
                query = query.Where(p => criteria.Shapes.Contains(p.Shape));
            }

            // Sauce Filter
            if (criteria.Sauces != null && criteria.Sauces.Any())
            {
                query = query.Where(p => criteria.Sauces.Contains(p.BaseSauce));
            }

            // Price Range Filter
            if (criteria.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= criteria.MinPrice.Value);
            }

            if (criteria.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= criteria.MaxPrice.Value);
            }

            // Sorting
            query = criteria.SortBy switch
            {
                SortOptionEnum.PriceAsc => query.OrderBy(p => p.Price),
                SortOptionEnum.PriceDesc => query.OrderByDescending(p => p.Price),
                SortOptionEnum.NameAsc => query.OrderBy(p => p.Name),
                SortOptionEnum.NameDesc => query.OrderByDescending(p => p.Name),
                _ => query.OrderBy(p => p.Name) // Default
            };

            // Pagination - Fetch data with anonymous type first
            var totalCount = await query.CountAsync();
            var pizzaData = await query
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    BrandName = p.Menu.Pizzeria.Brand!.Name,
                    p.Description,
                    p.Price,
                    p.ImageUrl,
                    p.WeightGrams,
                    p.Kcal,
                    p.DiameterCm,
                    p.WidthCm,
                    p.LengthCm,
                    p.Shape,
                    p.Style,
                    IngredientNames = p.Ingredients.Select(i => i.Name).ToList()
                })
                .ToListAsync();

            // Calculate PricePerSqCm in memory
            var pizzas = pizzaData.Select(p =>
            {
                double area;
                if (p.Shape == PizzaShapeEnum.Round)
                {
                    // Area = π * (diameter/2)²
                    var radius = p.DiameterCm / 2.0;
                    area = Math.PI * radius * radius;
                }
                else // Rectangle
                {
                    // Area = width * length
                    area = p.WidthCm * p.LengthCm;
                }

                // Prevent division by zero
                var pricePerSqCm = area > 0 ? (decimal)p.Price / (decimal)area : 0m;

                return new PizzaSearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BrandName = p.BrandName,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    WeightGrams = p.WeightGrams,
                    Kcal = p.Kcal,
                    DiameterCm = p.Shape == PizzaShapeEnum.Round ? p.DiameterCm : null,
                    StyleName = p.Style.GetDescription(),
                    PricePerSqCm = Math.Round(pricePerSqCm, 4),
                    IngredientNames = p.IngredientNames
                };
            }).ToList();

            return Ok(pizzas);
        }
    }
}