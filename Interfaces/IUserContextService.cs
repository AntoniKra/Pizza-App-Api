using System.Security.Claims;

namespace PizzaApp.Interfaces
{
    public interface IUserContextService
    {
        Guid? GetUserId();
        ClaimsPrincipal? User { get; }
    }
}
