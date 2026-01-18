using System.ComponentModel;

namespace PizzaApp.Enums
{
    public enum SauceTypeEnum
    {
        [Description("Pomidorowy")]
        Tomato = 1,
        [Description("Śmietanowy (Biały)")]
        Cream = 2,
        [Description("BBQ")]
        BBQ = 3,
        [Description("Pesto")]
        Pesto = 4,
        [Description("Ostry pomidorowy")]
        SpicyArrabbiata = 5,
        [Description("Kremow truflowy")]
        TruffleCream = 6
    }
}
