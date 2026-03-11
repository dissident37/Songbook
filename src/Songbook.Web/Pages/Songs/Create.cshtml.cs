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
public class CreateModel : PageModel
{
    private readonly SongbookDbContext _context;

    public CreateModel(SongbookDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    // Gefundene Artists (wird bei Query-Param "artistName" genutzt; Autocomplete läuft per JSON)
    public List<Artist> FoundArtists { get; set; } = new();

    public class InputModel
    {
        [Required]
        public string Title { get; set; } = "";

        [Required]
        public string ArtistName { get; set; } = "";

        public int? SelectedArtistId { get; set; }

        [Required]
        public string Content { get; set; } = "";

        // Wenn true: Song ist für alle sichtbar
        public bool IsPublic { get; set; } = false;
    }

    public async Task OnGetAsync(int? artistId, string? artistName)
    {
        if (artistId.HasValue)
        {
            var artist = await _context.Artists.FindAsync(artistId.Value);
            if (artist != null)
            {
                Input.ArtistName = artist.Name;
                Input.SelectedArtistId = artist.Id;
            }

            // Wichtig: wenn artistId gesetzt ist, brauchen wir keine Suche
            return;
        }

        if (!string.IsNullOrWhiteSpace(artistName))
        {
            var term = artistName.Trim();

            FoundArtists = await _context.Artists
                .Where(a => a.Name.ToLower().Contains(term.ToLower()))
                .OrderBy(a => a.Name)
                .ToListAsync();

            Input.ArtistName = term;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Allgemeine Model-Validierung: verhindert DB-Fehler (z.B. Title/Content = NULL)
        if (!ModelState.IsValid)
            return Page();

        // Eingabe einmal normalisieren (Trim), damit wir konsistent prüfen/suchen
        var name = (Input.ArtistName ?? "").Trim();

        // Wenn kein bestehender Artist gewählt wurde, muss ein neuer Name vorhanden sein
        if (!Input.SelectedArtistId.HasValue && string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError("Input.ArtistName",
                "Bitte einen Interpreten wählen oder einen neuen Namen eingeben.");
            return Page();
        }

        Artist artist;

        if (Input.SelectedArtistId.HasValue)
        {
            // Bestehenden Artist übernehmen (aus Autocomplete ausgewählt)
            artist = await _context.Artists.FindAsync(Input.SelectedArtistId.Value)
                     ?? throw new Exception("Artist not found");
        }
        else
        {
            // Neuen Artist: zuerst Duplikat prüfen (case-insensitive Vergleich)
            var normalized = name.ToUpperInvariant();

            // FirstOrDefaultAsync liefert Artist? (nullable) → deswegen extra Variable
            Artist? existingArtist = await _context.Artists
                .FirstOrDefaultAsync(a => a.Name.ToUpper() == normalized);

            if (existingArtist != null)
            {
                artist = existingArtist;
            }
            else
            {
                // Kein Duplikat gefunden → neuen Artist anlegen
                artist = new Artist { Name = name };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();
            }
        }


        // Song erstellen und Ownership setzen
        var song = new Song
        {
            Title = Input.Title,
            Content = Input.Content,
            ArtistId = artist.Id,
            IsPublic = Input.IsPublic,
            CreatedByUserId = User.GetProfileId()
        };

        _context.Songs.Add(song);
        await _context.SaveChangesAsync();

        // Redirect auf Artist-Details (Index/Details bleiben für Navigation sichtbar)
        return RedirectToPage("/Artists/Details", new { id = artist.Id });
    }

    public async Task<IActionResult> OnGetSearchArtistsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new JsonResult(Array.Empty<object>());

        var t = term.Trim();

        // Autocomplete: kleine Ergebnisliste für Dropdown-Vorschläge
        var artists = await _context.Artists
            .Where(a => a.Name.ToLower().Contains(t.ToLower()))
            .OrderBy(a => a.Name)
            .Take(5)
            .Select(a => new { a.Id, a.Name })
            .ToListAsync();

        return new JsonResult(artists);
    }
}
