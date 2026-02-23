using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;
using Songbook.Web.Auth;

namespace Songbook.Web.Pages.Songs;

public class DetailsModel : PageModel
{
    private readonly SongbookDbContext _context;

    public DetailsModel(SongbookDbContext context)
    {
        _context = context;
    }

    public Song? Song { get; set; }
    public bool IsAdmin => User.IsInRole("Admin");

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Sichtbarkeit:
        // - Gast: nur öffentliche Songs
        // - Eingeloggt: öffentliche + eigene private Songs
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var myProfileId = isAuthenticated ? User.GetProfileId() : (int?)null;

        var query = _context.Songs
            .Include(s => s.Artist)
            .Include(s => s.CreatedByUser)
            .Where(s => s.Id == id);

        if (!IsAdmin)
        {
            query = query.Where(s => s.IsPublic || (isAuthenticated && s.CreatedByUserId == myProfileId));
        }

        Song = await query.FirstOrDefaultAsync();

        if (Song == null)
            return NotFound();

        return Page();
    }

}
