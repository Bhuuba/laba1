using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NewMyApp.Web.Models;

public class CreatePostViewModel
{
    [Required(ErrorMessage = "Поле 'Текст' обов'язкове")]
    [MinLength(1, ErrorMessage = "Текст не може бути порожнім")]
    [Display(Name = "Контент")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Група")]
    public int? GroupId { get; set; }

    [Display(Name = "Зображення")]
    public IFormFile? ImageFile { get; set; }

    public string? ImageUrl { get; set; }
} 