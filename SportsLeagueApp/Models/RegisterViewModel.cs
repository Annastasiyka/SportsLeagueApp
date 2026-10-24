using System.ComponentModel.DataAnnotations;

namespace SportsLeagueApp.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Введіть ім'я та прізвище")]
        [Display(Name = "Ім'я та прізвище")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть електронну пошту")]
        [EmailAddress(ErrorMessage = "Некоректний формат пошти")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження пароля")]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}