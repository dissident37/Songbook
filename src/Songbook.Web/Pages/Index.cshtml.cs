using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;

namespace Songbook.Web.Pages;

public class IndexModel : PageModel
{
    private readonly SongbookDbContext _context;

    public IndexModel(SongbookDbContext context)
    {
        _context = context;
    }

    public List<Song> Songs { get; set; } = new();

    public async Task OnGetAsync()
    {
        Songs = await _context.Songs
            .Include(s => s.Artist)
            .ToListAsync();
    }
}
