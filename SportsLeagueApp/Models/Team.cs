using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace SportsLeagueApp.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва команди є обов'язковою")]
        public string? Name { get; set; }

        public string? City { get; set; }

        public int FoundedYear { get; set; }

        public string? CoachName { get; set; }

        public string? HomeStadium { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();
    }
}