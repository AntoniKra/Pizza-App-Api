namespace PizzaApp.DTOs
{
    public class WorkScheduleDto
    {
        public Guid Id { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan OpenTime { get; set; }
        public TimeSpan CloseTime { get; set; }
        public Guid PizzeriaId { get; set; }
        public string? PizzeriaName { get; set; }
    }
}
