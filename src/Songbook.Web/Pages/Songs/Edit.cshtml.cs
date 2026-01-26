using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Auth;
using Songbook.Web.Data;
using Songbook.Web.Models;
using System.ComponentModel.DataAnnotations;

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
        [Required]
        public string Title { get; set; } = "";

        // Anzeige im UI (Autocomplete). Entscheidend ist SelectedArtistId.
        [Required]
        public string ArtistName { get; set; } = "";

        // Muss aus Autocomplete kommen (nur vorhandene Artists)
        [Required]
        public int? SelectedArtistId { get; set; }

        [Required]
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

        // Ownership: nur der Besitzer darf editieren
        var myProfileId = User.GetProfileId();
        if (song.CreatedByUserId != myProfileId)
            return Forbid();

        Input.Title = song.Title;
        Input.ArtistName = song.Artist.Name;
        Input.SelectedArtistId = song.ArtistId;
        Input.Content = song.Content;
        Input.IsPublic = song.IsPublic;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        SongId = id;

        if (!ModelState.IsValid)
            return Page();

        var song = await _context.Songs.FindAsync(id);
        if (song == null)
            return NotFound();

        // Ownership: nur der Besitzer darf speichern
        var myProfileId = User.GetProfileId();
        if (song.CreatedByUserId != myProfileId)
            return Forbid();

        // In Edit darf kein neuer Artist angelegt werden: nur vorhandene auswählen
        if (!Input.SelectedArtistId.HasValue)
        {
            ModelState.AddModelError("Input.ArtistName",
                "Bitte einen vorhandenen Interpreten aus der Liste auswählen.");
            return Page();
        }

        var artistExists = await _context.Artists
            .AnyAsync(a => a.Id == Input.SelectedArtistId.Value);

        if (!artistExists)
        {
            ModelState.AddModelError("Input.ArtistName",
                "Der ausgewählte Interpret existiert nicht (bitte erneut auswählen).");
            return Page();
        }

        song.Title = Input.Title.Trim();
        song.ArtistId = Input.SelectedArtistId.Value;
        song.Content = Input.Content;
        song.IsPublic = Input.IsPublic;

        // Plaintext-Version für Suche/Anzeige (Akkord-Tags entfernen)
        song.ContentPlain = System.Text.RegularExpressions.Regex.Replace(
            Input.Content, @"\[[^\]]+\]", "");

        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnGetSearchArtistsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new JsonResult(Array.Empty<object>());

        var t = term.Trim();

        // Autocomplete-Vorschläge (nur vorhandene Artists)
        var artists = await _context.Artists
            .Where(a => a.Name.ToLower().Contains(t.ToLower()))
            .OrderBy(a => a.Name)
            .Take(5)
            .Select(a => new { a.Id, a.Name })
            .ToListAsync();

        return new JsonResult(artists);
    }
}
