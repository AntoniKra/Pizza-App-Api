using System.ComponentModel;

namespace PizzaApp.Enums
{
    public enum CrustThicknessEnum
    {
        [Description("Cienkie")]
        Thin = 1,
        [Description("Tradycyjne")]
        Medium = 2,
        [Description("Grube")]
        Thick = 3,
        [Description("Z wypełnionymi brzegami")]
        Stuffed = 4
    }
}
