
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsLeagueApp.Models;
using SportsLeagueApp.Data;

public class PlayersController : Controller
{
    private readonly AppDbContext _context;

    public PlayersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: PLAYERS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Players.ToListAsync());
    }

    // GET: PLAYERS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var player = await _context.Players
            .FirstOrDefaultAsync(m => m.Id == id);
        if (player == null)
        {
            return NotFound();
        }

        return View(player);
    }

    // GET: PLAYERS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: PLAYERS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,DateOfBirth,Position,JerseyNumber,Teams")] Player player)
    {
        if (ModelState.IsValid)
        {
            _context.Add(player);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(player);
    }

    // GET: PLAYERS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var player = await _context.Players.FindAsync(id);
        if (player == null)
        {
            return NotFound();
        }
        return View(player);
    }

    // POST: PLAYERS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("Id,FirstName,LastName,DateOfBirth,Position,JerseyNumber,Teams")] Player player)
    {
        if (id != player.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(player);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerExists(player.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(player);
    }

    // GET: PLAYERS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var player = await _context.Players
            .FirstOrDefaultAsync(m => m.Id == id);
        if (player == null)
        {
            return NotFound();
        }

        return View(player);
    }

    // POST: PLAYERS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player != null)
        {
            _context.Players.Remove(player);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PlayerExists(int? id)
    {
        return _context.Players.Any(e => e.Id == id);
    }
}
