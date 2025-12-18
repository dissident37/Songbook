using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;

namespace Songbook.Web.Pages.Songs;


public class CreateModel : PageModel
{
    private readonly SongbookDbContext _context;

    public CreateModel(SongbookDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    // найденные артисты
    public List<Artist> FoundArtists { get; set; } = new();

    public class InputModel
    {
        public string Title { get; set; } = "";
        public string ArtistName { get; set; } = "";
        public int? SelectedArtistId { get; set; }
        public string Content { get; set; } = "";
    }

    public async Task OnGetAsync(string? artistName)
    {
        if (!string.IsNullOrWhiteSpace(artistName))
        {
            FoundArtists = await _context.Artists
                .Where(a => a.Name.ToLower().Contains(artistName.ToLower()))
                .OrderBy(a => a.Name)
                .ToListAsync();

            Input.ArtistName = artistName;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Artist artist;

        if (Input.SelectedArtistId.HasValue)
        {
            // пользователь выбрал существующего артиста
            artist = await _context.Artists.FindAsync(Input.SelectedArtistId.Value)
                     ?? throw new Exception("Artist not found");
        }
        else
        {
            // создаём нового артиста
            artist = new Artist
            {
                Name = Input.ArtistName.Trim()
            };

            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
        }

        var song = new Song
        {
            Title = Input.Title,
            Content = Input.Content,
            ArtistId = artist.Id
        };

        _context.Songs.Add(song);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Artists/Details", new { id = artist.Id });
    }

    public async Task<IActionResult> OnGetSearchArtistsAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new JsonResult(Array.Empty<object>());

        var artists = await _context.Artists
            .Where(a => a.Name.ToLower().Contains(term.ToLower()))
            .OrderBy(a => a.Name)
            .Take(5)
            .Select(a => new { a.Id, a.Name })
            .ToListAsync();

        return new JsonResult(artists);
    }
}
