using System.ComponentModel;

namespace PizzaApp.Enums
{
    public enum SortOptionEnum
    {
        [Description("Domyœlnie")]
        Default = 1,
        [Description("Cena: od najni¿szej")]
        PriceAsc = 2,
        [Description("Cena: od najwy¿szej")]
        PriceDesc = 3,
        [Description("Nazwa: A-Z")]
        NameAsc = 4,
        [Description("Nazwa: Z-A")]
        NameDesc = 5
    }
}
