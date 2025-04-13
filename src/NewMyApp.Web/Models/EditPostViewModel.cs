using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NewMyApp.Web.Models
{
    public class EditPostViewModel
    {
        [Required(ErrorMessage = "Будь ласка, введіть текст поста")]
        public string Content { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
} 