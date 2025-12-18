using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;

namespace Songbook.Web.Pages.Artists;

public class IndexModel : PageModel
{
    private readonly SongbookDbContext _context;

    public IndexModel(SongbookDbContext context)
    {
        _context = context;
    }

    public IList<Artist> Artists { get; set; } = new List<Artist>();

    public async Task OnGetAsync()
    {
        Artists = await _context.Artists
            .Include(a => a.Songs)   // ← КЛЮЧЕВО
            .OrderBy(a => a.Name)
            .ToListAsync();
    }
}
