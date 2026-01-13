using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PizzaApp.Data;
using PizzaApp.DTOs;
using PizzaApp.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PizzaApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.Accounts.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("Użytkownik o takim adresie email już istnieje.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            Account account;

            if (dto.IsOwner)
            {
                if (string.IsNullOrWhiteSpace(dto.TaxId))
                    return BadRequest("Właściciel musi podać NIP (TaxId).");

                account = new Owner
                {
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PasswordHash = passwordHash,
                    TaxId = dto.TaxId
                };
            }
            else
            {
                account = new Customer
                {
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PasswordHash = passwordHash
                };
            }

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok("Rejestracja zakończona sukcesem.");
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Błędny email lub hasło.");
            }

            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(Account user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

            string role = user is Owner ? "Owner" : "Customer";

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), 
                new Claim(JwtRegisteredClaimNames.Email, user.Email),      
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("role", role) 
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}