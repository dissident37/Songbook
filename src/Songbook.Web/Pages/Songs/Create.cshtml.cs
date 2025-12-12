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
    public SongInputModel Input { get; set; } = new();

    public class SongInputModel
    {
        public string Title { get; set; } = "";
        public string ArtistName { get; set; } = "";
        public string Content { get; set; } = "";
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // --- Найти или создать исполнителя ---
        var artist = await _context.Artists
            .FirstOrDefaultAsync(a => a.Name == Input.ArtistName);

        if (artist == null)
        {
            artist = new Artist { Name = Input.ArtistName };
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync(); // Нужно, чтобы появился Id
        }

        // --- Создаём ContentPlain ---
        var contentPlain = RemoveChords(Input.Content);

        var song = new Song
        {
            Title = Input.Title,
            ArtistId = artist.Id,
            Content = Input.Content,
            ContentPlain = contentPlain,
            CreatedByUserId = 1 // временно, пока нет авторизации
        };

        _context.Songs.Add(song);
        await _context.SaveChangesAsync();

        return RedirectToPage("Index");
    }

    private string RemoveChords(string content)
    {
        // Очень простой очиститель: убираем аккорды вида [Am], [G], etc.
        return System.Text.RegularExpressions.Regex.Replace(content, @"\[[^\]]+\]", "");
    }
}
