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

    public Song Song { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Song = await _context.Songs
            .Include(s => s.Artist)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (Song == null)
            return NotFound();

        // Sichtbarkeit prüfen:
        // - Gast: nur public
        // - Eingeloggt: public + eigene private
        if (!Song.IsPublic)
        {
            if (User.Identity?.IsAuthenticated != true)
                return NotFound();

            var myProfileId = User.GetProfileId();
            if (Song.CreatedByUserId != myProfileId)
                return NotFound();
        }

        return Page();
    }
}
