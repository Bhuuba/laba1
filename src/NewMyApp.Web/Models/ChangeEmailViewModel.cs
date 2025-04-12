using System.ComponentModel.DataAnnotations;

namespace NewMyApp.Web.Models
{
    public class ChangeEmailViewModel
    {
        [Required(ErrorMessage = "Поточна електронна пошта обов'язкова")]
        [Display(Name = "Поточна електронна пошта")]
        public required string CurrentEmail { get; set; }

        [Required(ErrorMessage = "Нова електронна пошта обов'язкова")]
        [EmailAddress(ErrorMessage = "Невірний формат електронної пошти")]
        [Display(Name = "Нова електронна пошта")]
        public required string NewEmail { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public required string Password { get; set; }
    }
} 