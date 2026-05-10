using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;
using Songbook.Web.Auth;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace Songbook.Web.Pages.Songs;

public class DetailsModel : PageModel
{
    private readonly SongbookDbContext _context;

    public DetailsModel(SongbookDbContext context)
    {
        _context = context;
    }

    public Song? Song { get; set; }
    public bool IsAdmin => User.IsInRole("Admin");
    public List<string> SongChordNames { get; set; } = new();
    public Dictionary<string, List<Chord>> ChordDiagrams { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        // Sichtbarkeit:
        // - Gast: nur öffentliche Songs
        // - Eingeloggt: öffentliche + eigene private Songs
        var isAuthenticated = User.Identity?.IsAuthenticated == true;
        var myProfileId = isAuthenticated ? User.GetProfileId() : (int?)null;

        var query = _context.Songs
            .Include(s => s.Artist)
            .Include(s => s.CreatedByUser)
            .Where(s => s.Id == id);

        if (!IsAdmin)
        {
            query = query.Where(s =>
                !s.IsHiddenByAdmin &&
                (s.IsPublic || (isAuthenticated && s.CreatedByUserId == myProfileId)));
        }

        Song = await query.FirstOrDefaultAsync();

        if (Song == null)
            return NotFound();

        var matches = Regex.Matches(Song.Content, @"\[([^\]]+)\]");
        SongChordNames = matches
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (SongChordNames.Count > 0)
        {
            var chords = await _context.Chords
                .Where(c => SongChordNames.Contains(c.Name))
                .OrderBy(c => c.Id)
                .ToListAsync();
            ChordDiagrams = chords
                .GroupBy(c => c.Name)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        return Page();
    }

    public IHtmlContent RenderContent()
    {
        if (Song == null) return HtmlString.Empty;
        var pattern = new Regex(@"\[([^\]]+)\]");
        var sb = new StringBuilder();
        int last = 0;

        foreach (Match m in pattern.Matches(Song.Content))
        {
            if (m.Index > last)
                sb.Append(HtmlEncoder.Default.Encode(Song.Content[last..m.Index]));

            var name = HtmlEncoder.Default.Encode(m.Groups[1].Value);
            sb.Append($"""<span class="chord" data-chord="{name}">{name}</span>""");
            last = m.Index + m.Length;
        }

        if (last < Song.Content.Length)
            sb.Append(HtmlEncoder.Default.Encode(Song.Content[last..]));

        return new HtmlString(sb.ToString());
    }

}
