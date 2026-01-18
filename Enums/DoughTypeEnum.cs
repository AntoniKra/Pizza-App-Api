using System.ComponentModel;

namespace PizzaApp.Enums
{
    public enum DoughTypeEnum
    {
        [Description("Pszenne")]
        Wheat = 1,
        [Description("Pełnoziarniste")]
        WholeGrain = 2,
        [Description("Bezglutenowe")]
        GlutenFree = 3,
        [Description("Na zakwasie")]
        Sourdough = 4,
    }
}
