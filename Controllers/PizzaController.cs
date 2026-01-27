using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Entities;
using PizzaApp.Enums;
using PizzaApp.Extensions;
using PizzaApp.Services;
using System.IO;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PizzaController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;

        // SECURITY: Limit wielkości pliku (5MB)
        private const long MaxFileSize = 5 * 1024 * 1024;

        public PizzaController(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        [HttpPost]
        public async Task<ActionResult> CreatePizza([FromForm] CreatePizzaDto dto)
        {
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

            // WALIDACJA I UPLOAD PLIKU
            string? uploadedImageUrl = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var (isValid, errorMessage) = ValidateImageFile(dto.ImageFile);

                if (!isValid)
                {
                    return BadRequest($"Błąd pliku: {errorMessage}");
                }

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

        [HttpGet("{id}")]
        public async Task<ActionResult<PizzaDetailsDto>> GetPizza(Guid id)
        {
            var pizza = await _context.Pizzas
                .Include(p => p.Ingredients)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pizza == null)
            {
                return NotFound($"Wybrana pizza nie istnieje.");
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePizza(Guid id, [FromBody] UpdatePizzaDto dto)
        {
            var pizza = await _context.Pizzas
                .Include(p => p.Ingredients)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pizza == null) return NotFound("Wybrana pizza nie istnieje.");

            if (!await _context.Menus.AnyAsync(m => m.Id == dto.MenuId))
                return BadRequest("Wybrane menu nie istnieje.");

            var nameTaken = await _context.Pizzas
                .AnyAsync(p => p.MenuId == dto.MenuId && p.Name == dto.Name && p.Id != id);

            if (nameTaken) return Conflict("Inna pizza w tym menu ma już taką nazwę.");

            if (dto.Shape == PizzaShapeEnum.Round && (dto.DiameterCm == null || dto.DiameterCm <= 0))
                return BadRequest("Dla pizzy okrągłej wymagana jest średnica.");

            if (dto.Shape == PizzaShapeEnum.Rectangle && (dto.WidthCm == null || dto.LengthCm == null))
                return BadRequest("Dla pizzy prostokątnej wymagane są wymiary boków.");

            var newIngredients = await _context.Ingredients
                .Where(i => dto.IngredientIds.Contains(i.Id))
                .ToListAsync();

            if (newIngredients.Count != dto.IngredientIds.Count)
                return BadRequest("Jeden lub więcej podanych składników nie istnieje.");

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

            pizza.Ingredients = newIngredients;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePizza(Guid id)
        {
            var pizza = await _context.Pizzas.FindAsync(id);
            if (pizza == null) return NotFound("Wybrana pizza nie istnieje.");

            _context.Pizzas.Remove(pizza);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("search")]
        [Produces("application/json")]
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

            query = query.Where(p => p.Menu.Pizzeria.Address!.City!.Id.ToString() == criteria.CityId);
            query = query.Where(p => p.Menu.IsActive);

            if (criteria.BrandIds != null && criteria.BrandIds.Any())
                query = query.Where(p => criteria.BrandIds.Contains(p.Menu.Pizzeria.BrandId));

            if (criteria.Styles != null && criteria.Styles.Any())
                query = query.Where(p => criteria.Styles.Contains(p.Style));

            if (criteria.Doughs != null && criteria.Doughs.Any())
                query = query.Where(p => criteria.Doughs.Contains(p.Dough));

            if (criteria.Thicknesses != null && criteria.Thicknesses.Any())
                query = query.Where(p => criteria.Thicknesses.Contains(p.Thickness));

            if (criteria.Shapes != null && criteria.Shapes.Any())
                query = query.Where(p => criteria.Shapes.Contains(p.Shape));

            if (criteria.Sauces != null && criteria.Sauces.Any())
                query = query.Where(p => criteria.Sauces.Contains(p.BaseSauce));

            if (criteria.MinPrice.HasValue)
                query = query.Where(p => p.Price >= criteria.MinPrice.Value);

            if (criteria.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= criteria.MaxPrice.Value);

            query = criteria.SortBy switch
            {
                SortOptionEnum.PriceAsc => query.OrderBy(p => p.Price),
                SortOptionEnum.PriceDesc => query.OrderByDescending(p => p.Price),
                SortOptionEnum.NameAsc => query.OrderBy(p => p.Name),
                SortOptionEnum.NameDesc => query.OrderByDescending(p => p.Name),
                _ => query.OrderBy(p => p.Name)
            };

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

        // ==========================================================
        //              WALIDACJA PLIKU (SECURITY)
        // ==========================================================
        private (bool IsValid, string? ErrorMessage) ValidateImageFile(IFormFile file)
        {
            // 1. rozmiar (Hard Limit)
            if (file.Length > MaxFileSize)
            {
                return (false, $"Plik jest za duży. Maksymalny rozmiar to {MaxFileSize / 1024 / 1024}MB.");
            }

            // 2. rozszerzenie (Whitelist)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return (false, "Niedozwolony format pliku. Akceptujemy tylko: .jpg, .png, .webp.");
            }

            // 3. MIME Type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, "Nieprawidłowy typ zawartości (MIME).");
            }

            // 4. Magic Bytes (zabezpieczenie przed zmianą nazwy .exe na .png)
            try
            {
                using var stream = file.OpenReadStream();
                var header = new byte[4];
                stream.ReadExactly(header, 0, 4);

                var isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
                var isPng = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;
                // WebP (uproszczone): sprawdzenie czy nie jest pusty i czy RIFF istnieje (bajty 0-3)
                var isRiff = header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46;

                if (!isJpeg && !isPng && !isRiff)
                {
                    return (false, "Plik wygląda na uszkodzony lub ma fałszywe rozszerzenie.");
                }
            }
            catch
            {
                return (false, "Nie udało się zweryfikować pliku.");
            }

            return (true, null);
        }
    }
}