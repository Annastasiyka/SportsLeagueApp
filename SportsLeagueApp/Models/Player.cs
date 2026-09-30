using System.ComponentModel.DataAnnotations;

namespace SportsLeagueApp.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ім'я є обов'язковим")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Прізвище є обов'язковим")]
        public string? LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? Position { get; set; }

        public int JerseyNumber { get; set; }

        public List<Team> Teams { get; set; } = new List<Team>();
    }
}