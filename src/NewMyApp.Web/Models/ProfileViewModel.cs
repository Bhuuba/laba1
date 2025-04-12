using System.ComponentModel.DataAnnotations;

namespace NewMyApp.Web.Models;

public class ProfileViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [Display(Name = "Ім'я")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Прізвище обов'язкове")]
    [Display(Name = "Прізвище")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Фото профілю")]
    public string? ProfilePicture { get; set; }

    [Display(Name = "Про себе")]
    [MaxLength(500, ErrorMessage = "Біографія не може перевищувати 500 символів")]
    public string? Bio { get; set; }

    [Display(Name = "Дата народження")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    public int TotalPosts { get; set; }
    public int TotalFriends { get; set; }
    public int TotalGroups { get; set; }
} 