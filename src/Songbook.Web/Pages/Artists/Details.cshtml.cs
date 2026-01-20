using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;
using Songbook.Web.Auth;


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
        // 1) Artist ohne Songs laden
        Artist = await _context.Artists
            .FirstOrDefaultAsync(a => a.Id == id);

        if (Artist == null)
            return NotFound();

        // 2) Songs separat laden, aber mit Sichtbarkeits-Regeln
        var songsQuery = _context.Songs
            .Where(s => s.ArtistId == id)
            .AsQueryable();

        // Gast: nur public
        if (User.Identity?.IsAuthenticated != true)
        {
            songsQuery = songsQuery.Where(s => s.IsPublic);
        }
        else
        {
            // Eingeloggt: public + eigene private
            var myProfileId = User.GetProfileId();
            songsQuery = songsQuery.Where(s => s.IsPublic || s.CreatedByUserId == myProfileId);
        }

        // 3) In die Navigation-Property legen (damit Razor weiter wie раньше funktioniert)
        Artist.Songs = await songsQuery.ToListAsync();

        return Page();
    }

}
