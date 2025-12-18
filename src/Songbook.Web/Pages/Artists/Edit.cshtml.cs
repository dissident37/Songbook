using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;

namespace Songbook.Web.Pages.Artists;

public class EditModel : PageModel
{
    private readonly SongbookDbContext _context;

    public EditModel(SongbookDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Artist Artist { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Artist = await _context.Artists.FindAsync(id);
        if (Artist == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        _context.Attach(Artist).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
