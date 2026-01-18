using System.ComponentModel;

namespace PizzaApp.Enums
{
    public enum PizzaStyleEnum
    {
        [Description("Neapolitańska")]
        Neapolitan = 1,
        [Description("Amerykańska")]
        American = 2,
        [Description("Chicago Style")]
        ChicagoStyle = 3,
        [Description("Sycylijska")]
        Sicilian = 4,
        [Description("Rzymska")]
        Roman = 5,
        [Description("Calzone")]
        Calzone = 6,
    }
}
