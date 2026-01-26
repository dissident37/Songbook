using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;
using Microsoft.AspNetCore.Authorization;


namespace Songbook.Web.Pages.Artists;

[Authorize(Policy = "AdminOnly")]
public class CreateModel : PageModel
{
    private readonly SongbookDbContext _context;

    public CreateModel(SongbookDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Artist Artist { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        _context.Artists.Add(Artist);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
