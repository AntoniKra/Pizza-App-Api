using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json.Serialization;

namespace PizzaApp.Entities
{
    public abstract class Account
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }

        [JsonIgnore] 
        public required string PasswordHash { get; set; }
    }

}