using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(6)]
        public required string Password { get; set; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        public bool IsOwner { get; set; } = false;

        public string? TaxId { get; set; }
    }
}
