using System.ComponentModel.DataAnnotations;

namespace NewMyApp.Web.Models;

public class CreatePostViewModel
{
    [Required(ErrorMessage = "Контент обов'язковий")]
    [MinLength(1, ErrorMessage = "Контент не може бути порожнім")]
    [Display(Name = "Контент")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Група")]
    public int? GroupId { get; set; }

    [Display(Name = "Зображення")]
    public IFormFile? Image { get; set; }

    public string? ImageUrl { get; set; }
} 