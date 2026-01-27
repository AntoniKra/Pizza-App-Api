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
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;


        public MenuController(AppDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;

        }

        // GET: api/Menu/{id}
        // Pobiera szczegóły jednego menu wraz z pizzami
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuDetailsDto>> GetMenu(Guid id)
        {
            var menu = await _context.Menus
                .Include(m => m.Pizzas)
                    .ThenInclude(p => p.Ingredients)
                .Include(m => m.Pizzeria)
                    .ThenInclude(pz => pz.Brand)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null) return NotFound("Menu nie istnieje.");

            var dto = new MenuDetailsDto
            {
                Id = menu.Id,
                Name = menu.Name,
                Description = menu.Description,
                IsActive = menu.IsActive,
                PizzasCount = menu.Pizzas.Count,
                Pizzas = menu.Pizzas.Select(p => new PizzaSearchResultDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    IngredientNames = p.Ingredients.Select(i => i.Name).ToList()
                }).ToList()
            };

            return Ok(dto);
        }

        // POST: api/Menu/Create
        // Tworzy nowe, puste menu dla pizzerii
        [HttpPost("Create")]
        [Authorize]
        public async Task<ActionResult> CreateMenu([FromBody] CreateMenuDto dto)
        {
            var pizzeria = await _context.Pizzerias
                .Include(p => p.Brand)
                    .ThenInclude(b => b.Owner)
                .FirstOrDefaultAsync(p => p.Id.ToString() == dto.Pizzeria.Id);
            if (pizzeria is null)
                return BadRequest("Wybrana pizzeria nie istnieje.");

            var userId = _userContextService.GetUserId();
            if (pizzeria.Brand.Owner.Id != userId) return Forbid();


            var menu = new Menu
            {
                PizzeriaId = pizzeria.Id,
                Name = dto.Name,
                Description = dto.Description,
                IsActive = false
            };

            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMenu), new { id = menu.Id }, new Menu
            {
                Id = menu.Id,
                Name = menu.Name,
                Description = menu.Description,
                IsActive = menu.IsActive
            });
        }


        // PUT: api/Menu/{id}/activate
        // Dedykowana metoda do ustawiania menu jako główne
        [HttpPut("{id}/activate")]
        [Authorize]
        public async Task<IActionResult> ActivateMenu(Guid id)
        {
            var menu = await _context.Menus
                .Include(m => m.Pizzeria)
                    .ThenInclude(p => p.Brand)
                        .ThenInclude(b => b.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (menu == null) return NotFound("Menu nie istnieje.");

            if (menu.Pizzeria.Brand.Owner.Id != _userContextService.GetUserId())
                return Forbid();

            if (menu.IsActive) return Ok("To menu jest już aktywne.");

            // Wyłączamy wszystkie inne menu tej pizzerii
            await DeactivateOtherMenus(menu.PizzeriaId, menu.Id);

            menu.IsActive = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Menu/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteMenu(Guid id)
        {
            var menu = await _context.Menus
                .Include(m => m.Pizzas)
                .Include(m => m.Pizzeria)
                    .ThenInclude(p => p.Brand)
                        .ThenInclude(b => b.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (menu == null) return NotFound("Menu nie istnieje.");

            if (menu.Pizzeria.Brand.Owner.Id != _userContextService.GetUserId())
                return Forbid();

            if (menu.IsActive)
            {
                return BadRequest("Nie można usunąć aktywnego menu. Najpierw aktywuj inne.");
            }

            // Jeśli menu zawiera pizze, Entity Framework może rzucić wyjątek zależnie od konfiguracji kaskadowania.
            // W AppDbContext nie widać explicite Cascade Delete dla Menu->Pizza, 
            // ale Pizza ma wymagane MenuId, więc usunięcie Menu usunie też pizze (Cascade jest domyślne dla required foreign key).

            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Menu/{pizzeriaId}
        // Pobiera wszystkie karty menu dla danej pizzerii
        [HttpGet("GetAllMenus/{pizzeriaId}")]
        public async Task<ActionResult<IEnumerable<MenuListItemDto>>> GetMenusForPizzeria(Guid pizzeriaId)
        {
            var menus = await _context.Menus
                .Where(m => m.PizzeriaId == pizzeriaId)
                .Select(m => new MenuListItemDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    IsActive = m.IsActive,
                    PizzasCount = m.Pizzas.Count
                })
                .ToListAsync();

            if (!menus.Any())
            {
                var pizzeriaExists = await _context.Pizzerias.AnyAsync(p => p.Id == pizzeriaId);
                if (!pizzeriaExists) return NotFound("Pizzeria nie istnieje.");
            }

            return Ok(menus);
        }





        // Metoda pomocnicza
        private async Task DeactivateOtherMenus(Guid pizzeriaId, Guid currentMenuId)
        {
            var otherActiveMenus = await _context.Menus
                .Where(m => m.PizzeriaId == pizzeriaId && m.Id != currentMenuId && m.IsActive)
                .ToListAsync();

            foreach (var m in otherActiveMenus)
            {
                m.IsActive = false;
            }
        }
    }
}