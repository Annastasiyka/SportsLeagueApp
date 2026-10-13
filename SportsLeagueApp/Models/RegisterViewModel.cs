using System.ComponentModel.DataAnnotations;

namespace SportsLeagueApp.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Заповніть електронну пошту")]
        [EmailAddress(ErrorMessage = "Некоректний формат email")]
        [Display(Name = "Електронна пошта")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердіть пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження пароля")]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}