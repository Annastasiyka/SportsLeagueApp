using System;

namespace SportsLeagueApp.Models
{
    public class PlayerDraftDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Position { get; set; }
        public int? JerseyNumber { get; set; }
        public int[] SelectedTeamIds { get; set; } = System.Array.Empty<int>();
    }
}