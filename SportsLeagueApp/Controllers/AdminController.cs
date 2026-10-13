using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsLeagueApp.Models;

namespace SportsLeagueApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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

        // GET: /Admin/ManageRoles?userId=...
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
    }
}