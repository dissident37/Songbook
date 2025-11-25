using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;

namespace Songbook.Web.Pages.Songs;

public class DeleteModel : PageModel
{
    private readonly SongbookDbContext _context;

    public DeleteModel(SongbookDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Song Song { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var s = await _context.Songs.FirstOrDefaultAsync(x => x.Id == id);

        if (s == null)
            return NotFound();

        Song = s;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var s = await _context.Songs.FindAsync(Song.Id);

        if (s == null)
            return NotFound();

        _context.Songs.Remove(s);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
