using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsLeagueApp.Data;
using SportsLeagueApp.Models;

namespace SportsLeagueApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Admin/Index
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesList = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRolesList.Add(new UserRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? user.UserName ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return View(userRolesList);
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (user.Email?.ToLower() == "admin@league.com")
                {
                    TempData["Error"] = "Головного адміністратора видалити неможливо!";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Користувача успішно видалено з бази даних.";
                }
                else
                {
                    TempData["Error"] = "Помилка при видаленні користувача.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/ManageRoles
        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new ManageUserRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = allRoles.Select(role => new RoleSelection
                {
                    RoleName = role.Name!,
                    IsSelected = userRoles.Contains(role.Name!)
                }).ToList()
            };

            return View(model);
        }

        // POST: /Admin/ManageRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(ManageUserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName);

            var resultRemove = await _userManager.RemoveFromRolesAsync(user, userRoles);
            if (!resultRemove.Succeeded)
            {
                ModelState.AddModelError("", "Не вдалося видалити попередні ролі.");
                return View(model);
            }

            var resultAdd = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!resultAdd.Succeeded)
            {
                ModelState.AddModelError("", "Не вдалося додати нові ролі.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }


        // GET: /Admin/ChatAccessRequests
        [HttpGet]
        public async Task<IActionResult> ChatAccessRequests()
        {
            var requests = await _context.TeamChatAccesses.ToListAsync();
            ViewBag.Teams = await _context.Teams.ToDictionaryAsync(t => t.Id, t => t.Name);
            return View(requests);
        }

        // POST: /Admin/ApproveAccess
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAccess(int id)
        {
            var request = await _context.TeamChatAccesses.FindAsync(id);
            if (request != null)
            {
                request.Status = "Approved";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Доступ схвалено.";
            }
            return RedirectToAction(nameof(ChatAccessRequests));
        }

        // POST: /Admin/RejectAccess
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAccess(int id)
        {
            var request = await _context.TeamChatAccesses.FindAsync(id);
            if (request != null)
            {
                request.Status = "Rejected";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Заявку відхилено.";
            }
            return RedirectToAction(nameof(ChatAccessRequests));
        }

        // POST: /Admin/RevokeChatAccess (Вилучити доступ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeChatAccess(int id)
        {
            var accessRecord = await _context.TeamChatAccesses.FindAsync(id);
            if (accessRecord != null)
            {
                _context.TeamChatAccesses.Remove(accessRecord);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Доступ до чату успішно вилучено. Користувач більше не зможе читати/писати у цьому чаті.";
            }
            return RedirectToAction(nameof(ChatAccessRequests));
        }
    }
}