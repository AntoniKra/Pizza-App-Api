using PizzaApp.DTOs;

namespace PizzaApp.Extensions
{
    public static class LookUpExtensions
    {
        public static LookUpItemDto ToLookUpItemDto(this Enum value)
        {
            return new LookUpItemDto
            {
                Id = value.ToString(),
                Name = value.GetDescription()
            };
        }
    }
}