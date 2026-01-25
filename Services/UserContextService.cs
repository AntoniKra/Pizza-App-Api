using Microsoft.IdentityModel.JsonWebTokens;
using PizzaApp.Interfaces;
using System.Security.Claims;

namespace PizzaApp.Services
{
    public class UserContextService: IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return null;
            }

            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)
                          ?? user.FindFirst(JwtRegisteredClaimNames.Sub)
                          ?? user.FindFirst("sub");

            if (idClaim != null && Guid.TryParse(idClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}