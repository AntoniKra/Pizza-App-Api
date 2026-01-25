using System.ComponentModel.DataAnnotations;

namespace PizzaApp.DTOs
{
    public class CreateWorkScheduleDto
    {
        [Required(ErrorMessage = "Dzieñ tygodnia jest wymagany")]
        [Range(0, 6, ErrorMessage = "Dzieñ tygodnia musi byæ w zakresie 0-6 (0 = niedziela, 6 = sobota)")]
        public required int DayOfWeek { get; set; }

        [Required(ErrorMessage = "Godzina otwarcia jest wymagana")]
        public required TimeSpan OpenTime { get; set; }

        [Required(ErrorMessage = "Godzina zamkniêcia jest wymagana")]
        public required TimeSpan CloseTime { get; set; }

        [Required(ErrorMessage = "ID pizzerii jest wymagane")]
        public required Guid PizzeriaId { get; set; }
    }
}
