using System.ComponentModel.DataAnnotations;

namespace NewMyApp.Web.Models;

public class CreateCommentViewModel
{
    [Required(ErrorMessage = "Контент обов'язковий")]
    [MinLength(1, ErrorMessage = "Контент не може бути порожнім")]
    [Display(Name = "Контент")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ідентифікатор поста обов'язковий")]
    public int PostId { get; set; }

    public int? ParentCommentId { get; set; }
} 