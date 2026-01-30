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
            Console.WriteLine($"Name: {dto.Name}");
            Console.WriteLine($"Price: {dto.Price}");
            Console.WriteLine($"WeightGrams (DTO): {dto.WeightGrams}");
            Console.WriteLine($"Kcal (DTO): {dto.Kcal}");
            Console.WriteLine($"DiameterCm (DTO): {dto.DiameterCm}");

            if (dto.Style != null) Console.WriteLine($"Style ID: {dto.Style.Id}");
            else Console.WriteLine("Style is NULL!");
            var menuExists = await _context.Menus.AnyAsync(m => m.Id == dto.MenuId);
            if (!menuExists)
            {
                return BadRequest("Wybrane menu nie istnieje lub jest nieprawidłowe.");
            }

            if (await _context.Pizzas.AnyAsync(p => p.MenuId == dto.MenuId && p.Name == dto.Name))
            {
                return Conflict($"Pizza '{dto.Name}' już istnieje w tym menu.");
            }

            // Parsowanie
            if (!Enum.TryParse<PizzaStyleEnum>(dto.Style.Id, true, out var styleEnum))
            {
                return BadRequest($"Nieprawidłowe ID stylu: {dto.Style.Id}");
            }

            if (!Enum.TryParse<DoughTypeEnum>(dto.Dough.Id, true, out var doughEnum))
            {
                return BadRequest($"Nieprawidłowe ID ciasta: {dto.Dough.Id}");
            }

            if (!Enum.TryParse<SauceTypeEnum>(dto.BaseSauce.Id, true, out var sauceEnum))
            {
                return BadRequest($"Nieprawidłowe ID sosu: {dto.BaseSauce.Id}");
            }

            if (!Enum.TryParse<CrustThicknessEnum>(dto.Thickness.Id, true, out var thicknessEnum))
            {
                return BadRequest($"Nieprawidłowe ID grubości: {dto.Thickness.Id}");
            }

            if (!Enum.TryParse<PizzaShapeEnum>(dto.Shape.Id, true, out var shapeEnum))
            {
                return BadRequest($"Nieprawidłowe ID kształtu: {dto.Shape.Id}");
            }


            // WALIDACJA WYMIARÓW
            if (shapeEnum == PizzaShapeEnum.Round)
            {
                if (dto.DiameterCm == null || dto.DiameterCm <= 0)
                    return BadRequest("Dla pizzy okrągłej wymagana jest średnica.");
                dto.WidthCm = null;
                dto.LengthCm = null;
            }
            else if (shapeEnum == PizzaShapeEnum.Rectangle)
            {
                if (dto.WidthCm == null || dto.LengthCm == null)
                    return BadRequest("Dla pizzy prostokątnej wymagane są boki.");
                dto.DiameterCm = null;
            }

            // WALIDACJA I UPLOAD PLIKU
            string? uploadedImageUrl = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var (isValid, error) = ValidateImageFile(dto.ImageFile);
                if (!isValid) return BadRequest(error);

                uploadedImageUrl = await _fileService.UploadFileAsync(dto.ImageFile, "menu-items");
            }
            // ZAPIS DO BAZY
            var existingIngredients = await _context.Ingredients
                .Where(i => dto.IngredientIds.Contains(i.Id))
                .ToListAsync();

            if (existingIngredients.Count != dto.IngredientIds.Count) return BadRequest("Błędne składniki.");

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

                // SPAROWANE ENUMY
                Style = styleEnum,
                Dough = doughEnum,
                BaseSauce = sauceEnum,
                Thickness = thicknessEnum,
                Shape = shapeEnum,

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
                .Include(p => p.Menu).ThenInclude(m => m.Pizzeria).ThenInclude(pz => pz.Brand)
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
                    p.Style, // Tu pobieramy Enum z bazy
                    IngredientNames = p.Ingredients.Select(i => i.Name).ToList()
                })
                .ToListAsync();

            var pizzas = pizzaData.Select(p =>
            {
                double area = (p.Shape == PizzaShapeEnum.Round)
                    ? Math.PI * Math.Pow(p.DiameterCm / 2.0, 2)
                    : p.WidthCm * p.LengthCm;

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
                    Style = p.Style.ToLookUpItemDto(), // Zwracamy pełny obiekt {id, name}

                    PricePerSqCm = area > 0 ? Math.Round((decimal)p.Price / (decimal)area, 4) : 0m,
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
        public async Task<IActionResult> UpdatePizza(Guid id, [FromForm] UpdatePizzaDto dto)
        {
            var pizza = await _context.Pizzas
                .Include(p => p.Ingredients)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pizza == null) return NotFound("Wybrana pizza nie istnieje.");

            // PARSOWANIE ENUMÓW
            if (!Enum.TryParse<PizzaStyleEnum>(dto.Style.Id, true, out var styleEnum))
                return BadRequest($"Nieprawidłowe ID stylu: {dto.Style.Id}");

            if (!Enum.TryParse<DoughTypeEnum>(dto.Dough.Id, true, out var doughEnum))
                return BadRequest($"Nieprawidłowe ID ciasta: {dto.Dough.Id}");

            if (!Enum.TryParse<SauceTypeEnum>(dto.BaseSauce.Id, true, out var sauceEnum))
                return BadRequest($"Nieprawidłowe ID sosu: {dto.BaseSauce.Id}");

            if (!Enum.TryParse<CrustThicknessEnum>(dto.Thickness.Id, true, out var thicknessEnum))
                return BadRequest($"Nieprawidłowe ID grubości: {dto.Thickness.Id}");

            if (!Enum.TryParse<PizzaShapeEnum>(dto.Shape.Id, true, out var shapeEnum))
                return BadRequest($"Nieprawidłowe ID kształtu: {dto.Shape.Id}");

            // WALIDACJA WYMIARÓW
            if (shapeEnum == PizzaShapeEnum.Round && (dto.DiameterCm == null || dto.DiameterCm <= 0))
                return BadRequest("Dla pizzy okrągłej wymagana jest średnica.");

            if (shapeEnum == PizzaShapeEnum.Rectangle && (dto.WidthCm == null || dto.LengthCm == null))
                return BadRequest("Dla pizzy prostokątnej wymagane są boki.");

            // WALIDACJA I UPLOAD NOWEGO PLIKU (jeśli dostarczony)
            string? newImageUrl = pizza.ImageUrl; // Zachowaj stary jeśli nie ma nowego
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                var (isValid, error) = ValidateImageFile(dto.ImageFile);
                if (!isValid) return BadRequest(error);

                // Upload nowego pliku
                newImageUrl = await _fileService.UploadFileAsync(dto.ImageFile, "menu-items");

                // Opcjonalnie: usuń stary plik (jeśli był)
                if (!string.IsNullOrEmpty(pizza.ImageUrl))
                {
                    try
                    {
                        await _fileService.DeleteFileAsync(pizza.ImageUrl);
                    }
                    catch
                    {
                        // Ignoruj błędy usuwania starego pliku
                    }
                }
            }

            // WALIDACJA SKŁADNIKÓW
            var newIngredients = await _context.Ingredients
                .Where(i => dto.IngredientIds.Contains(i.Id))
                .ToListAsync();

            if (newIngredients.Count != dto.IngredientIds.Count)
                return BadRequest("Jeden lub więcej podanych składników nie istnieje.");

            // AKTUALIZACJA ENCJI
            pizza.Name = dto.Name;
            pizza.Description = dto.Description;
            pizza.ImageUrl = newImageUrl; // Nowy lub stary URL
            pizza.Price = dto.Price;
            pizza.WeightGrams = dto.WeightGrams;
            pizza.Kcal = dto.Kcal;

            // PRZYPISANIE SPAROWANYCH ENUMÓW
            pizza.Style = styleEnum;
            pizza.Dough = doughEnum;
            pizza.BaseSauce = sauceEnum;
            pizza.Thickness = thicknessEnum;
            pizza.Shape = shapeEnum;

            // WYMIARY
            if (shapeEnum == PizzaShapeEnum.Round)
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

        [HttpPost("search")]
        [Produces("application/json")]
        public async Task<ActionResult<List<PizzaSearchResultDto>>> SearchPizzas([FromBody] PizzaSearchCriteriaDto criteria)
        {
            // --- DIAGNOSTYKA ---
            Console.WriteLine($"Search: City={criteria.CityId}, Sort={criteria.SortBy}");

            var query = _context.Pizzas
                .Include(p => p.Ingredients)
                .Include(p => p.Menu).ThenInclude(m => m.Pizzeria).ThenInclude(pz => pz.Address).ThenInclude(a => a.City)
                .Include(p => p.Menu).ThenInclude(m => m.Pizzeria).ThenInclude(pz => pz.Brand)
                .AsQueryable();

            // 1. FILTRY PODSTAWOWE
            query = query.Where(p => p.Menu.Pizzeria.Address!.City!.Id.ToString() == criteria.CityId);
            query = query.Where(p => p.Menu.IsActive);

            if (criteria.BrandIds != null && criteria.BrandIds.Any())
                query = query.Where(p => criteria.BrandIds.Contains(p.Menu.Pizzeria.BrandId));

            if (criteria.MinDiameter.HasValue)
            {
                query = query.Where(p =>
                    p.Shape == PizzaShapeEnum.Rectangle ||
                    (p.Shape == PizzaShapeEnum.Round && p.DiameterCm >= criteria.MinDiameter.Value)
                );
            }

            // 2. FILTRY ZAAWANSOWANE (ENUMY)
            try
            {
                if (criteria.Styles != null && criteria.Styles.Any())
                {
                    var styleEnums = criteria.Styles.Select(s => Enum.Parse<PizzaStyleEnum>(s.Id, true)).ToList();
                    query = query.Where(p => styleEnums.Contains(p.Style));
                }
                if (criteria.Doughs != null && criteria.Doughs.Any())
                {
                    var doughEnums = criteria.Doughs.Select(d => Enum.Parse<DoughTypeEnum>(d.Id, true)).ToList();
                    query = query.Where(p => doughEnums.Contains(p.Dough));
                }
                if (criteria.Thicknesses != null && criteria.Thicknesses.Any())
                {
                    var thicknessEnums = criteria.Thicknesses.Select(t => Enum.Parse<CrustThicknessEnum>(t.Id, true)).ToList();
                    query = query.Where(p => thicknessEnums.Contains(p.Thickness));
                }
                if (criteria.Sauces != null && criteria.Sauces.Any())
                {
                    var sauceEnums = criteria.Sauces.Select(s => Enum.Parse<SauceTypeEnum>(s.Id, true)).ToList();
                    query = query.Where(p => sauceEnums.Contains(p.BaseSauce));
                }
                // Shape filter handles checkboxes
                if (criteria.Shapes != null && criteria.Shapes.Any())
                {
                    var shapeEnums = criteria.Shapes.Select(s => Enum.Parse<PizzaShapeEnum>(s.Id, true)).ToList();
                    query = query.Where(p => shapeEnums.Contains(p.Shape));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd filtrów: {ex.Message}");
            }

            // 3. CENA
            if (criteria.MinPrice.HasValue) query = query.Where(p => p.Price >= criteria.MinPrice.Value);
            if (criteria.MaxPrice.HasValue) query = query.Where(p => p.Price <= criteria.MaxPrice.Value);

            // 4. SORTOWANIE W BAZIE (Tylko proste pola: Cena, Nazwa)
            // Sortowanie wyliczane (Opłacalność, Kcal) robimy PÓŹNIEJ
            switch (criteria.SortBy)
            {
                case SortOptionEnum.PriceAsc: query = query.OrderBy(p => p.Price); break;
                case SortOptionEnum.PriceDesc: query = query.OrderByDescending(p => p.Price); break;
                case SortOptionEnum.NameAsc: query = query.OrderBy(p => p.Name); break;
                case SortOptionEnum.NameDesc: query = query.OrderByDescending(p => p.Name); break;
                // Default: po nazwie, jeśli nie wybrano sortowania specjalnego
                default:
                    if ((int)criteria.SortBy < 6) query = query.OrderBy(p => p.Name);
                    break;
            }

            // 5. POBRANIE DANYCH (Materializacja do pamięci)
            // UWAGA: Nie robimy tu jeszcze Skip/Take, jeśli sortujemy po polach wyliczanych!
            var pizzaDataList = await query.ToListAsync();

            // 6. MAPOWANIE I WYLICZENIA
            var result = pizzaDataList.Select(p =>
            {
                // Pole powierzchni
                double area = (p.Shape == PizzaShapeEnum.Round)
                    ? Math.PI * Math.Pow(p.DiameterCm / 2.0, 2)
                    : p.WidthCm * p.LengthCm;

                // Opłacalność (zł / cm2)
                decimal pricePerSqCm = area > 0 ? (decimal)p.Price / (decimal)area : 0m;

                // Gęstość (kcal / g) - Zabezpieczenie przed dzieleniem przez zero
                double kcalPerGram = p.WeightGrams > 0 ? (double)p.Kcal / p.WeightGrams : 0;

                return new PizzaSearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    BrandName = p.Menu.Pizzeria.Brand!.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    WeightGrams = p.WeightGrams,
                    Kcal = p.Kcal,
                    DiameterCm = p.Shape == PizzaShapeEnum.Round ? p.DiameterCm : null,
                    Style = p.Style.ToLookUpItemDto(),
                    IngredientNames = p.Ingredients.Select(i => i.Name).ToList(),

                    // Pola wyliczane
                    PricePerSqCm = Math.Round(pricePerSqCm, 4),
                    KcalPerGram = Math.Round(kcalPerGram, 2) // Zaokrąglamy do 2 miejsc
                };
            }).ToList();

            // 7. SORTOWANIE W PAMIĘCI (Dla pól wyliczanych)
            switch (criteria.SortBy)
            {
                case SortOptionEnum.ProfitabilityAsc: // Najtańsza za cm2
                    result = result.OrderBy(p => p.PricePerSqCm).ToList();
                    break;

                case SortOptionEnum.KcalDensityDesc: // MASA (Dużo kcal w małej wadze)
                    result = result.OrderByDescending(p => p.KcalPerGram).ToList();
                    break;

                case SortOptionEnum.KcalDensityAsc: // REDUKCJA (Mało kcal w dużej wadze - objętościówka)
                    result = result.OrderBy(p => p.KcalPerGram).ToList();
                    break;
            }

            // 8. PAGINACJA (Ręczna, bo mamy listę w pamięci)
            var pagedResult = result
                .Skip((criteria.PageNumber - 1) * criteria.PageSize)
                .Take(criteria.PageSize)
                .ToList();

            return Ok(pagedResult);
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