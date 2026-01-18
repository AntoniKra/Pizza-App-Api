using System.ComponentModel;

namespace PizzaApp.Enums
{
    public enum PizzaShapeEnum
    {
        [Description("Okrągła")]
        Round = 1,
        [Description("Prostokątna")]
        Rectangle = 2
    }
}
