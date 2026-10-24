namespace SportsLeagueApp.Models
{
    public class TeamChatAccess
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;    
        public string UserEmail { get; set; } = string.Empty;  
        public int TeamId { get; set; }                       

        public string Status { get; set; } = "Pending";
    }
}