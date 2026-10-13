using System.ComponentModel.DataAnnotations;

namespace SportsLeagueApp.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Заповніть електронну пошту")]
        [EmailAddress(ErrorMessage = "Некоректний формат email")]
        [Display(Name = "Електронна пошта")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запам'ятати мене")]
        public bool RememberMe { get; set; }
    }
}