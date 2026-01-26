using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;
using Microsoft.AspNetCore.Authorization;


namespace Songbook.Web.Pages.Songs;

[Authorize]
public class EditModel : PageModel
{
    private readonly SongbookDbContext _context;

    public EditModel(SongbookDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public SongInputModel Input { get; set; } = new();

    public int SongId { get; set; }

    public class SongInputModel
    {
        public string Title { get; set; } = "";
        public string ArtistName { get; set; } = "";
        public string Content { get; set; } = "";

        // Wenn true: sichtbar für alle
        public bool IsPublic { get; set; } = false;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        SongId = id;

        var song = await _context.Songs
            .Include(s => s.Artist)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (song == null)
            return NotFound();

        Input.Title = song.Title;
        Input.ArtistName = song.Artist.Name;
        Input.Content = song.Content;
        Input.IsPublic = song.IsPublic;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var song = await _context.Songs.FindAsync(id);
        if (song == null)
            return NotFound();

        // обновление артиста
        var artist = await _context.Artists
            .FirstOrDefaultAsync(a => a.Name == Input.ArtistName);

        if (artist == null)
        {
            artist = new Artist { Name = Input.ArtistName };
            await _context.Artists.AddAsync(artist);
            await _context.SaveChangesAsync();
        }

        song.Title = Input.Title;
        song.ArtistId = artist.Id;
        song.Content = Input.Content;
        song.IsPublic = Input.IsPublic;
        song.ContentPlain = System.Text.RegularExpressions.Regex.Replace(Input.Content, @"\[[^\]]+\]", "");

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}
