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
    public class TeamsController : Controller
    {
        private readonly AppDbContext _context;

        public TeamsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Teams
        public async Task<IActionResult> Index()
        {
            var teams = await _context.Teams.Include(t => t.Players).ToListAsync();
            return View(teams);
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var team = await _context.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (team == null) return NotFound();

            return View(team);
        }

        // GET: Teams/Create
        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create(bool restoreDraft = false)
        {
            string? jsonDraft = HttpContext.Session.GetString("TeamCreateDraft");
            Team teamToDisplay = new Team();

            if (!string.IsNullOrEmpty(jsonDraft))
            {
                if (restoreDraft)
                {
                    var draftDto = JsonSerializer.Deserialize<TeamDraftDto>(jsonDraft);
                    if (draftDto != null)
                    {
                        teamToDisplay = new Team
                        {
                            Name = draftDto.Name,
                            SportCategory = draftDto.SportCategory,
                            City = draftDto.City,
                            CoachName = draftDto.CoachName,
                            FoundedYear = draftDto.FoundedYear,
                            HomeStadium = draftDto.HomeStadium,
                            Players = _context.Players
                                .Where(p => draftDto.SelectedPlayerIds.Contains(p.Id))
                                .ToList()
                        };
                    }
                }
                else
                {
                    ViewBag.HasDraft = true;
                }
            }

            ViewBag.AllPlayers = _context.Players.ToList();
            return View(teamToDisplay);
        }

        // POST: Teams/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([Bind("Id,Name,SportCategory,City,FoundedYear,CoachName,HomeStadium")] Team team, int[] selectedPlayers)
        {
            var draftDto = new TeamDraftDto
            {
                Name = team.Name,
                SportCategory = team.SportCategory,
                City = team.City,
                CoachName = team.CoachName,
                FoundedYear = team.FoundedYear,
                HomeStadium = team.HomeStadium,
                SelectedPlayerIds = selectedPlayers ?? Array.Empty<int>()
            };

            HttpContext.Session.SetString("TeamCreateDraft", JsonSerializer.Serialize(draftDto));

            if (selectedPlayers != null && selectedPlayers.Length > 0)
            {
                team.Players = _context.Players
                    .Where(p => selectedPlayers.Contains(p.Id) && p.SportCategory == team.SportCategory)
                    .ToList();
            }

            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();

                HttpContext.Session.Remove("TeamCreateDraft");
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AllPlayers = _context.Players.ToList();
            return View(team);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CancelCreate()
        {
            HttpContext.Session.Remove("TeamCreateDraft");
            return RedirectToAction(nameof(Index));
        }

        // GET: Teams/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id, bool restoreDraft = false)
        {
            if (id == null) return NotFound();

            var teamFromDb = await _context.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (teamFromDb == null) return NotFound();

            var originalDto = new TeamDraftDto
            {
                Id = teamFromDb.Id,
                Name = teamFromDb.Name,
                SportCategory = teamFromDb.SportCategory,
                City = teamFromDb.City,
                CoachName = teamFromDb.CoachName,
                FoundedYear = teamFromDb.FoundedYear,
                HomeStadium = teamFromDb.HomeStadium,
                SelectedPlayerIds = teamFromDb.Players.Select(p => p.Id).ToArray()
            };
            HttpContext.Session.SetString($"TeamOriginal_{id}", JsonSerializer.Serialize(originalDto));

            string draftKey = $"TeamEditDraft_{id}";
            string? jsonDraft = HttpContext.Session.GetString(draftKey);

            Team teamToDisplay = teamFromDb;

            if (!string.IsNullOrEmpty(jsonDraft))
            {
                if (restoreDraft)
                {
                    var draftDto = JsonSerializer.Deserialize<TeamDraftDto>(jsonDraft);
                    if (draftDto != null)
                    {
                        teamToDisplay = new Team
                        {
                            Id = teamFromDb.Id,
                            Name = draftDto.Name,
                            SportCategory = draftDto.SportCategory,
                            City = draftDto.City,
                            CoachName = draftDto.CoachName,
                            FoundedYear = draftDto.FoundedYear,
                            HomeStadium = draftDto.HomeStadium,
                            Players = _context.Players
                                .Where(p => draftDto.SelectedPlayerIds.Contains(p.Id))
                                .ToList()
                        };
                    }
                }
                else
                {
                    ViewBag.HasDraft = true;
                }
            }

            ViewBag.AllPlayers = _context.Players.ToList();
            return View(teamToDisplay);
        }

        // POST: Teams/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SportCategory,City,FoundedYear,CoachName,HomeStadium")] Team team, int[] selectedPlayers)
        {
            if (id != team.Id) return NotFound();

            var draftDto = new TeamDraftDto
            {
                Id = team.Id,
                Name = team.Name,
                SportCategory = team.SportCategory,
                City = team.City,
                CoachName = team.CoachName,
                FoundedYear = team.FoundedYear,
                HomeStadium = team.HomeStadium,
                SelectedPlayerIds = selectedPlayers ?? Array.Empty<int>()
            };

            string draftKey = $"TeamEditDraft_{id}";
            HttpContext.Session.SetString(draftKey, JsonSerializer.Serialize(draftDto));

            if (selectedPlayers != null && selectedPlayers.Length > 0)
            {
                team.Players = _context.Players
                    .Where(p => selectedPlayers.Contains(p.Id) && p.SportCategory == team.SportCategory)
                    .ToList();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var teamToUpdate = await _context.Teams
                        .Include(t => t.Players)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (teamToUpdate == null) return NotFound();

                    teamToUpdate.Name = team.Name;
                    teamToUpdate.SportCategory = team.SportCategory;
                    teamToUpdate.City = team.City;
                    teamToUpdate.FoundedYear = team.FoundedYear;
                    teamToUpdate.CoachName = team.CoachName;
                    teamToUpdate.HomeStadium = team.HomeStadium;

                    teamToUpdate.Players.Clear();
                    if (selectedPlayers != null && selectedPlayers.Length > 0)
                    {
                        var newPlayers = _context.Players
                            .Where(p => selectedPlayers.Contains(p.Id) && p.SportCategory == team.SportCategory)
                            .ToList();

                        foreach (var player in newPlayers)
                        {
                            teamToUpdate.Players.Add(player);
                        }
                    }

                    _context.Update(teamToUpdate);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Remove(draftKey);
                    HttpContext.Session.Remove($"TeamOriginal_{id}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.AllPlayers = _context.Players.ToList();
            return View(team);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult CancelEdit(int id)
        {
            HttpContext.Session.Remove($"TeamEditDraft_{id}");
            HttpContext.Session.Remove($"TeamOriginal_{id}");
            return RedirectToAction(nameof(Index));
        }

        // GET: Teams/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var team = await _context.Teams
                .Include(t => t.Players)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (team == null) return NotFound();

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.Id == id);
        }
    }
}