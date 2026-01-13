using System.ComponentModel.DataAnnotations;

namespace PizzaApp.Entities
{
    public class WorkSchedule
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public int DayOfWeek { get; set; } 
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }

        public Guid PizzeriaId { get; set; }
        public Pizzeria? Pizzeria { get; set; }
    }
}
