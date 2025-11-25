using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Songbook.Web.Data;
using Songbook.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Songbook.Web.Pages.Songs;

public class EditModel : PageModel
{
    private readonly SongbookDbContext _context;

    public EditModel(SongbookDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Song Song { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Song? s = await _context.Songs.FirstOrDefaultAsync(x => x.Id == id);

        if (s == null)
            return NotFound();

        Song = s;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _context.Songs.Update(Song);
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}
