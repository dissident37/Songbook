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
    public bool IsAdmin => User.IsInRole("Admin");

    public async Task OnGetAsync()
    {
            var query = _context.Songs
            .Include(s => s.Artist)
            .Include(s => s.CreatedByUser)
            .AsQueryable();

        if (!IsAdmin)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                query = query.Where(s => s.IsPublic && !s.IsHiddenByAdmin);
            }
            else
            {
                var myProfileId = User.GetProfileId();
                query = query.Where(s => !s.IsHiddenByAdmin && (s.IsPublic || s.CreatedByUserId == myProfileId));
            }
        }

        SongList = await query.ToListAsync();
    }
}
