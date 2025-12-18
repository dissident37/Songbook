using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
        Artist = await _context.Artists
            .Include(a => a.Songs)   // ← ВОТ ГЛАВНОЕ
            .FirstOrDefaultAsync(a => a.Id == id);

        if (Artist == null)
            return NotFound();

        return Page();
    }
}
