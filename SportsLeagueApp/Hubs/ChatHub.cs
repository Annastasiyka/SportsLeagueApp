using Microsoft.AspNetCore.SignalR;
using SportsLeagueApp.Data;
using SportsLeagueApp.Models;
using System;
using System.Threading.Tasks;

namespace SportsLeagueApp.Hubs
{
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string user, string message)
        {
            var chatMessage = new ChatMessage
            {
                SenderName = user,
                Content = message,
                Timestamp = DateTime.Now,
                TeamId = null
            };
            _context.ChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            await Clients.All.SendAsync("ReceiveGlobalMessage", user, message);
        }

        public async Task SendMessageToTeam(string teamIdStr, string user, string message)
        {
            if (int.TryParse(teamIdStr, out int teamId))
            {
                var chatMessage = new ChatMessage
                {
                    SenderName = user,
                    Content = message,
                    Timestamp = DateTime.Now,
                    TeamId = teamId
                };
                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();

                await Clients.Group("team_" + teamIdStr).SendAsync("ReceiveTeamMessage", user, message);
            }
        }

        public async Task JoinTeamGroup(string teamId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "team_" + teamId);
        }

        public async Task LeaveTeamGroup(string teamId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "team_" + teamId);
        }
    }
}