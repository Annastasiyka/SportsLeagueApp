using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsLeagueApp.Data;
using SportsLeagueApp.Models;

namespace SportsLeagueApp.Controllers
{
    public class PlayersController : Controller
    {
        private readonly AppDbContext _context;

        public PlayersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Players
        public async Task<IActionResult> Index()
        {
            var players = await _context.Players.Include(p => p.Teams).ToListAsync();
            return View(players);
        }

        // GET: Players/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players
                .Include(p => p.Teams)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (player == null) return NotFound();

            return View(player);
        }

        // GET: Players/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create(bool restoreDraft = false)
        {
            string? jsonDraft = HttpContext.Session.GetString("PlayerCreateDraft");
            Player playerToDisplay = new Player();

            if (!string.IsNullOrEmpty(jsonDraft))
            {
                if (restoreDraft)
                {
                    var draftDto = JsonSerializer.Deserialize<PlayerDraftDto>(jsonDraft);
                    if (draftDto != null)
                    {
                        playerToDisplay = new Player
                        {
                            FirstName = draftDto.FirstName,
                            LastName = draftDto.LastName,
                            DateOfBirth = draftDto.DateOfBirth,
                            SportCategory = draftDto.SportCategory,
                            Position = draftDto.Position,
                            JerseyNumber = draftDto.JerseyNumber,
                            Teams = _context.Teams
                                .Where(t => draftDto.SelectedTeamIds.Contains(t.Id))
                                .ToList()
                        };
                    }
                }
                else
                {
                    ViewBag.HasDraft = true;
                }
            }

            ViewBag.AllTeams = _context.Teams.ToList();
            return View(playerToDisplay);
        }

        // POST: Players/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,DateOfBirth,SportCategory,Position,JerseyNumber")] Player player, int[] selectedTeams)
        {
            var draftDto = new PlayerDraftDto
            {
                FirstName = player.FirstName,
                LastName = player.LastName,
                DateOfBirth = player.DateOfBirth,
                SportCategory = player.SportCategory,
                Position = player.Position,
                JerseyNumber = player.JerseyNumber,
                SelectedTeamIds = selectedTeams ?? Array.Empty<int>()
            };

            HttpContext.Session.SetString("PlayerCreateDraft", JsonSerializer.Serialize(draftDto));

            if (selectedTeams != null && selectedTeams.Length > 0)
            {
                player.Teams = _context.Teams
                    .Where(t => selectedTeams.Contains(t.Id) && t.SportCategory == player.SportCategory)
                    .ToList();
            }

            if (ModelState.IsValid)
            {
                _context.Add(player);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("PlayerCreateDraft");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AllTeams = _context.Teams.ToList();
            return View(player);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CancelCreate()
        {
            HttpContext.Session.Remove("PlayerCreateDraft");
            return RedirectToAction(nameof(Index));
        }

        // GET: Players/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id, bool restoreDraft = false)
        {
            if (id == null) return NotFound();

            var playerFromDb = await _context.Players
                .Include(p => p.Teams)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (playerFromDb == null) return NotFound();

            var originalDto = new PlayerDraftDto
            {
                Id = playerFromDb.Id,
                FirstName = playerFromDb.FirstName,
                LastName = playerFromDb.LastName,
                DateOfBirth = playerFromDb.DateOfBirth,
                SportCategory = playerFromDb.SportCategory,
                Position = playerFromDb.Position,
                JerseyNumber = playerFromDb.JerseyNumber,
                SelectedTeamIds = playerFromDb.Teams.Select(t => t.Id).ToArray()
            };
            HttpContext.Session.SetString($"PlayerOriginal_{id}", JsonSerializer.Serialize(originalDto));

            string draftKey = $"PlayerEditDraft_{id}";
            string? jsonDraft = HttpContext.Session.GetString(draftKey);

            Player playerToDisplay = playerFromDb;

            if (!string.IsNullOrEmpty(jsonDraft))
            {
                if (restoreDraft)
                {
                    var draftDto = JsonSerializer.Deserialize<PlayerDraftDto>(jsonDraft);
                    if (draftDto != null)
                    {
                        playerToDisplay = new Player
                        {
                            Id = playerFromDb.Id,
                            FirstName = draftDto.FirstName,
                            LastName = draftDto.LastName,
                            DateOfBirth = draftDto.DateOfBirth,
                            SportCategory = draftDto.SportCategory,
                            Position = draftDto.Position,
                            JerseyNumber = draftDto.JerseyNumber,
                            Teams = _context.Teams
                                .Where(t => draftDto.SelectedTeamIds.Contains(t.Id))
                                .ToList()
                        };
                    }
                }
                else
                {
                    ViewBag.HasDraft = true;
                }
            }

            ViewBag.AllTeams = _context.Teams.ToList();
            return View(playerToDisplay);
        }

        // POST: Players/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,DateOfBirth,SportCategory,Position,JerseyNumber")] Player player, int[] selectedTeams)
        {
            if (id != player.Id) return NotFound();

            var draftDto = new PlayerDraftDto
            {
                Id = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                DateOfBirth = player.DateOfBirth,
                SportCategory = player.SportCategory,
                Position = player.Position,
                JerseyNumber = player.JerseyNumber,
                SelectedTeamIds = selectedTeams ?? Array.Empty<int>()
            };

            string draftKey = $"PlayerEditDraft_{id}";
            HttpContext.Session.SetString(draftKey, JsonSerializer.Serialize(draftDto));

            if (selectedTeams != null && selectedTeams.Length > 0)
            {
                player.Teams = _context.Teams
                    .Where(t => selectedTeams.Contains(t.Id) && t.SportCategory == player.SportCategory)
                    .ToList();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var playerToUpdate = await _context.Players
                        .Include(p => p.Teams)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (playerToUpdate == null) return NotFound();

                    playerToUpdate.FirstName = player.FirstName;
                    playerToUpdate.LastName = player.LastName;
                    playerToUpdate.DateOfBirth = player.DateOfBirth;
                    playerToUpdate.SportCategory = player.SportCategory;
                    playerToUpdate.Position = player.Position;
                    playerToUpdate.JerseyNumber = player.JerseyNumber;

                    playerToUpdate.Teams.Clear();
                    if (selectedTeams != null && selectedTeams.Length > 0)
                    {
                        var newTeams = _context.Teams
                            .Where(t => selectedTeams.Contains(t.Id) && t.SportCategory == player.SportCategory)
                            .ToList();

                        foreach (var team in newTeams)
                        {
                            playerToUpdate.Teams.Add(team);
                        }
                    }

                    _context.Update(playerToUpdate);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove(draftKey);
                    HttpContext.Session.Remove($"PlayerOriginal_{id}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayerExists(player.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AllTeams = _context.Teams.ToList();
            return View(player);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CancelEdit(int id)
        {
            HttpContext.Session.Remove($"PlayerEditDraft_{id}");
            HttpContext.Session.Remove($"PlayerOriginal_{id}");
            return RedirectToAction(nameof(Index));
        }

        // GET: Players/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var player = await _context.Players
                .Include(p => p.Teams)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (player == null) return NotFound();

            return View(player);
        }

        // POST: Players/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                _context.Players.Remove(player);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlayerExists(int id)
        {
            return _context.Players.Any(e => e.Id == id);
        }
    }
}