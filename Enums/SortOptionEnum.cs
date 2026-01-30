using System.ComponentModel;

namespace PizzaApp.Enums
{
    public enum SortOptionEnum
    {
        [Description("Domyślnie")]
        Default = 1,
        [Description("Cena: od najniższej")]
        PriceAsc = 2,
        [Description("Cena: od najwyższej")]
        PriceDesc = 3,
        [Description("Nazwa: A-Z")]
        NameAsc = 4,
        [Description("Nazwa: Z-A")]
        NameDesc = 5,
        [Description("Opłacalność: Najlepsza (zł/cm²)")]
        ProfitabilityAsc = 6,

        [Description("Masa: Najwięcej kcal/g")]
        KcalDensityDesc = 7,

        [Description("Redukcja: Najmniej kcal/g")]
        KcalDensityAsc = 8

    }
}
