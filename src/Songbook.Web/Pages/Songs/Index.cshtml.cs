using Microsoft.AspNetCore.Mvc.RazorPages;
using Songbook.Web.Data;
using Songbook.Web.Models;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Auth;

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
            var query = _context.Songs
            .Include(s => s.Artist)
            .AsQueryable();

        // Nicht eingeloggt -> nur öffentliche Songs
        if (User.Identity?.IsAuthenticated != true)
        {
            query = query.Where(s => s.IsPublic);
        }
        else
        {
            // Eingeloggt -> öffentliche + eigene private Songs
            var myProfileId = User.GetProfileId();
            query = query.Where(s => s.IsPublic || s.CreatedByUserId == myProfileId);
        }

        SongList = await query.ToListAsync();
    }
}
