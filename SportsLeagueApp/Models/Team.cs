using System.ComponentModel.DataAnnotations;

namespace SportsLeagueApp.Models
{
    public class Team
    {
        public string? LogoUrl { get; set; }
        public int Id { get; set; }

        private string? _name;
        [Required(ErrorMessage = "Заповніть інформацію про назву команди")]
        public string? Name
        {
            get => _name;
            set => _name = CapitalizeFirst(value);
        }

        [Required(ErrorMessage = "Вид спорту є обов'язковим")]
        public string? SportCategory { get; set; }

        private string? _city;
        [Required(ErrorMessage = "Заповніть інформацію про місто")]
        public string? City
        {
            get => _city;
            set => _city = CapitalizeFirst(value);
        }

        [Required(ErrorMessage = "Вкажіть рік заснування")]
        [Range(1800, 2100, ErrorMessage = "Рік заснування має бути в межах від 1800 до 2100 (не може бути від'ємним)")]
        public int? FoundedYear { get; set; }

        private string? _coachName;
        [Required(ErrorMessage = "Заповніть інформацію про тренера")]
        public string? CoachName
        {
            get => _coachName;
            set => _coachName = CapitalizeFirst(value);
        }

        private string? _homeStadium;
        [Required(ErrorMessage = "Заповніть інформацію про домашній стадіон")]
        public string? HomeStadium
        {
            get => _homeStadium;
            set => _homeStadium = CapitalizeFirst(value);
        }

        public List<Player> Players { get; set; } = new List<Player>();

        private static string? CapitalizeFirst(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim();
            return char.ToUpper(s[0]) + (s.Length > 1 ? s.Substring(1) : string.Empty);
        }
    }
}