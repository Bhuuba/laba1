using System.ComponentModel.DataAnnotations;

namespace NewMyApp.Web.Models;

public class CreateGroupViewModel
{
    [Required(ErrorMessage = "Назва групи обов'язкова")]
    [StringLength(100, ErrorMessage = "Назва групи не може перевищувати 100 символів")]
    [Display(Name = "Назва групи")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Опис не може перевищувати 500 символів")]
    [Display(Name = "Опис")]
    public string? Description { get; set; }

    [Display(Name = "Обкладинка групи")]
    public IFormFile? CoverImage { get; set; }
} 