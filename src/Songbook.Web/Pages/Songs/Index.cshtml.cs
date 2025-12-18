using Microsoft.AspNetCore.Mvc.RazorPages;
using Songbook.Web.Data;
using Songbook.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Songbook.Web.Pages.Songs;

public class SongIndexModel : PageModel
{
    private readonly SongbookDbContext _context;

    public SongIndexModel(SongbookDbContext context)
    {
        _context = context;
    }

    public IList<Song> SongList { get; set; } = [];

    public async Task OnGetAsync()
    {
        SongList = await _context.Songs.Include(s => s.Artist).ToListAsync();
    }
}
