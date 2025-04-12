using System.ComponentModel.DataAnnotations;

namespace NewMyApp.Web.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Поточний пароль обов'язковий")]
        [DataType(DataType.Password)]
        [Display(Name = "Поточний пароль")]
        public required string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Новий пароль обов'язковий")]
        [StringLength(100, ErrorMessage = "Пароль повинен містити мінімум {2} символів", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Новий пароль")]
        public required string NewPassword { get; set; }

        [Required(ErrorMessage = "Підтвердження паролю обов'язкове")]
        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження нового паролю")]
        [Compare("NewPassword", ErrorMessage = "Паролі не співпадають")]
        public required string ConfirmPassword { get; set; }
    }
} 