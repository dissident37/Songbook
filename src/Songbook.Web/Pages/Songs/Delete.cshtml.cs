using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Auth;
using Songbook.Web.Data;
using Songbook.Web.Models;

namespace Songbook.Web.Pages.Songs;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly SongbookDbContext _context;

    public DeleteModel(SongbookDbContext context)
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

        var isAdmin = User.IsInRole("Admin");
        var myProfileId = User.GetProfileId();
        if (!isAdmin && Song.CreatedByUserId != myProfileId)
            return Forbid();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var song = await _context.Songs.FindAsync(id);
        if (song != null)
        {
            var isAdmin = User.IsInRole("Admin");
            var myProfileId = User.GetProfileId();
            if (!isAdmin && song.CreatedByUserId != myProfileId)
                return Forbid();

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("Index");
    }
}
