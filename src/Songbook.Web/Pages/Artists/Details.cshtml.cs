using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Auth;
using Songbook.Web.Data;
using Songbook.Web.Models;

namespace Songbook.Web.Pages.Artists;

public class DetailsModel : PageModel
{
    private readonly SongbookDbContext _context;

    public DetailsModel(SongbookDbContext context)
    {
        _context = context;
    }

    public Artist Artist { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Artist ohne Songs laden (Songs werden mit Sichtbarkeits-Regeln separat geladen)
        var artist = await _context.Artists
            .FirstOrDefaultAsync(a => a.Id == id);

        if (artist == null)
            return NotFound();

        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var myProfileId = isAuthenticated ? User.GetProfileId() : (int?)null;

        // Sichtbarkeit:
        // - Gast: nur öffentliche Songs
        // - Eingeloggt: öffentliche + eigene private Songs
        var songs = await _context.Songs
            .Where(s => s.ArtistId == id)
            .Where(s => s.IsPublic || (isAuthenticated && s.CreatedByUserId == myProfileId))
            .OrderBy(s => s.Title)
            .ToListAsync();

        // In Navigation-Property setzen, damit die Razor-View wie gewohnt funktioniert
        artist.Songs = songs;

        Artist = artist;
        return Page();
    }
}
