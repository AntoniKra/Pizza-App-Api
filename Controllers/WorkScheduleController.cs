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
    public class WorkScheduleController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContextService;

        public WorkScheduleController(AppDbContext context, IUserContextService userContextService)
        {
            _context = context;
            _userContextService = userContextService;
        }

        // GET: api/WorkSchedule/GetByPizzeria/{pizzeriaId}
        [HttpGet("GetByPizzeria/{pizzeriaId}")]
        public async Task<ActionResult<IEnumerable<WorkScheduleDto>>> GetWorkSchedulesByPizzeria(Guid pizzeriaId)
        {
            var pizzeriaExists = await _context.Pizzerias.AnyAsync(p => p.Id == pizzeriaId);
            if (!pizzeriaExists)
            {
                return NotFound("Pizzeria nie istnieje.");
            }

            var schedules = await _context.WorkSchedules
                .Where(ws => ws.PizzeriaId == pizzeriaId)
                .Include(ws => ws.Pizzeria)
                .Select(ws => new WorkScheduleDto
                {
                    Id = ws.Id,
                    DayOfWeek = ws.DayOfWeek,
                    OpenTime = ws.OpenTime,
                    CloseTime = ws.CloseTime,
                    PizzeriaId = ws.PizzeriaId,
                    PizzeriaName = ws.Pizzeria!.Name
                })
                .OrderBy(ws => ws.DayOfWeek)
                .ToListAsync();

            return Ok(schedules);
        }

        // GET: api/WorkSchedule/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<WorkScheduleDto>> GetWorkSchedule(Guid id)
        {
            var schedule = await _context.WorkSchedules
                .Include(ws => ws.Pizzeria)
                .FirstOrDefaultAsync(ws => ws.Id == id);

            if (schedule == null)
            {
                return NotFound("Harmonogram nie istnieje.");
            }

            var scheduleDto = new WorkScheduleDto
            {
                Id = schedule.Id,
                DayOfWeek = schedule.DayOfWeek,
                OpenTime = schedule.OpenTime,
                CloseTime = schedule.CloseTime,
                PizzeriaId = schedule.PizzeriaId,
                PizzeriaName = schedule.Pizzeria?.Name
            };

            return Ok(scheduleDto);
        }

        // POST: api/WorkSchedule
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<WorkScheduleDto>> CreateWorkSchedule(CreateWorkScheduleDto dto)
        {
            var pizzeria = await _context.Pizzerias
                .Include(p => p.Brand)
                    .ThenInclude(b => b.Owner)
                .FirstOrDefaultAsync(p => p.Id == dto.PizzeriaId);

            if (pizzeria == null)
            {
                return BadRequest("Pizzeria nie istnieje.");
            }

            var userId = _userContextService.GetUserId();
            if (pizzeria.Brand?.Owner?.Id != userId)
            {
                return Forbid();
            }

            if (dto.OpenTime >= dto.CloseTime)
            {
                return BadRequest("Godzina otwarcia musi byæ wczeœniejsza ni¿ godzina zamkniêcia.");
            }

            var scheduleExists = await _context.WorkSchedules
                .AnyAsync(ws => ws.PizzeriaId == dto.PizzeriaId && ws.DayOfWeek == dto.DayOfWeek);
            if (scheduleExists)
            {
                return Conflict($"Harmonogram dla tego dnia tygodnia ju¿ istnieje dla tej pizzerii.");
            }

            var schedule = new WorkSchedule
            {
                DayOfWeek = dto.DayOfWeek,
                OpenTime = dto.OpenTime,
                CloseTime = dto.CloseTime,
                PizzeriaId = dto.PizzeriaId
            };

            _context.WorkSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            var resultDto = new WorkScheduleDto
            {
                Id = schedule.Id,
                DayOfWeek = schedule.DayOfWeek,
                OpenTime = schedule.OpenTime,
                CloseTime = schedule.CloseTime,
                PizzeriaId = schedule.PizzeriaId,
                PizzeriaName = pizzeria.Name
            };

            return CreatedAtAction(nameof(GetWorkSchedule), new { id = resultDto.Id }, resultDto);
        }

        // PUT: api/WorkSchedule/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateWorkSchedule(Guid id, UpdateWorkScheduleDto dto)
        {
            var schedule = await _context.WorkSchedules
                .Include(ws => ws.Pizzeria)
                    .ThenInclude(p => p.Brand)
                        .ThenInclude(b => b.Owner)
                .FirstOrDefaultAsync(ws => ws.Id == id);

            if (schedule == null)
            {
                return NotFound("Harmonogram nie istnieje.");
            }

            var userId = _userContextService.GetUserId();
            if (schedule.Pizzeria?.Brand?.Owner?.Id != userId)
            {
                return Forbid();
            }

            if (dto.OpenTime >= dto.CloseTime)
            {
                return BadRequest("Godzina otwarcia musi byæ wczeœniejsza ni¿ godzina zamkniêcia.");
            }

            var duplicateExists = await _context.WorkSchedules
                .AnyAsync(ws => ws.Id != id && ws.PizzeriaId == schedule.PizzeriaId && ws.DayOfWeek == dto.DayOfWeek);
            if (duplicateExists)
            {
                return Conflict($"Harmonogram dla tego dnia tygodnia ju¿ istnieje dla tej pizzerii.");
            }

            schedule.DayOfWeek = dto.DayOfWeek;
            schedule.OpenTime = dto.OpenTime;
            schedule.CloseTime = dto.CloseTime;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WorkScheduleExists(id))
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

        // DELETE: api/WorkSchedule/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteWorkSchedule(Guid id)
        {
            var schedule = await _context.WorkSchedules
                .Include(ws => ws.Pizzeria)
                    .ThenInclude(p => p.Brand)
                        .ThenInclude(b => b.Owner)
                .FirstOrDefaultAsync(ws => ws.Id == id);

            if (schedule == null)
            {
                return NotFound("Harmonogram nie istnieje.");
            }

            var userId = _userContextService.GetUserId();
            if (schedule.Pizzeria?.Brand?.Owner?.Id != userId)
            {
                return Forbid();
            }

            _context.WorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WorkScheduleExists(Guid id)
        {
            return _context.WorkSchedules.Any(e => e.Id == id);
        }
    }
}
