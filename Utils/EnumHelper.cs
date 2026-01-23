using PizzaApp.DTOs;
using PizzaApp.Extensions;

namespace PizzaApp.Utils
{
    public static class EnumHelper
    {
        public static List<LookUpItemDto> GetAllValues<TEnum>() where TEnum : Enum
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<Enum>()
                .Select(e => e.ToLookUpItemDto())
                .ToList();
        }
    }
}
