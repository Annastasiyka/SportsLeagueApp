using System;

namespace SportsLeagueApp.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;    
        public string? FileUrl { get; set; }                   
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public int? TeamId { get; set; }
    }
}