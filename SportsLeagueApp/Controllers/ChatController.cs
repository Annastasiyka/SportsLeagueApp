using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsLeagueApp.Data;
using SportsLeagueApp.Models;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Collections.Generic;

namespace SportsLeagueApp.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ChatController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Global() => RedirectToAction("Index", "Home");
        public IActionResult TeamChat() => RedirectToAction("Index", "Teams");

        [HttpGet]
        public async Task<IActionResult> GetTeamChatData(int teamId)
        {
            var user = await _userManager.GetUserAsync(User);
            var team = await _context.Teams.FindAsync(teamId);
            if (team == null) return NotFound();

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            bool hasAccess = isAdmin || await _context.TeamChatAccesses
                .AnyAsync(a => a.UserId == user.Id && a.TeamId == teamId && a.Status == "Approved");

            var existingAccess = await _context.TeamChatAccesses
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.TeamId == teamId);

            var messages = new List<object>();
            if (hasAccess)
            {
                var dbMessages = await _context.ChatMessages
                    .Where(m => m.TeamId == teamId)
                    .OrderBy(m => m.Timestamp)
                    .Take(50)
                    .ToListAsync();

                messages = dbMessages.Select(m => {
                    var rawName = m.SenderName ?? "Гість";

                    if (rawName.ToLower() == "admin@league.com")
                    {
                        rawName = "Адмін";
                    }
                    else if (rawName.Contains("@"))
                    {
                        rawName = rawName.Split('@')[0];
                    }

                    return new
                    {
                        senderName = rawName,
                        timestamp = m.Timestamp.ToString("HH:mm"),
                        content = m.Content
                    };
                }).ToList<object>();
            }

            return Json(new
            {
                teamId = team.Id,
                teamName = team.Name,
                hasAccess = hasAccess,
                accessStatus = existingAccess?.Status,
                messages = messages
            });
        }

        [HttpPost]
        public async Task<IActionResult> RequestAccessAjax(int teamId)
        {
            var user = await _userManager.GetUserAsync(User);
            var existing = await _context.TeamChatAccesses
                .FirstOrDefaultAsync(a => a.UserId == user.Id && a.TeamId == teamId);

            if (existing == null)
            {
                var accessRequest = new TeamChatAccess
                {
                    UserId = user.Id,
                    UserEmail = user.Email ?? user.UserName ?? "Користувач",
                    TeamId = teamId,
                    Status = "Pending"
                };
                _context.TeamChatAccesses.Add(accessRequest);
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Json(new { success = true, url = "/uploads/" + uniqueFileName, name = file.FileName });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public async Task<IActionResult> ClearChat(int? teamId)
        {
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                var messagesToDelete = _context.ChatMessages.Where(m => m.TeamId == teamId);
                _context.ChatMessages.RemoveRange(messagesToDelete);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Немає прав" });
        }
    }
}