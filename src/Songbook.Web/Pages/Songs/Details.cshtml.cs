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

        var matches = Regex.Matches(Song.Content, ChordPattern, RegexOptions.None);
        SongChordNames = matches
            .Select(m => m.Value)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return Page();
    }

    // Aккорды: длинные варианты перед короткими, иначе Am7 матчится как Am
    private static readonly Regex ChordRegex = new(ChordPattern, RegexOptions.Compiled);
    private const string ChordPattern =
        @"(?<![a-zA-Z])(A#maj7|Abmaj7|Amaj7|A#m7|Abm7|Am7|A#7|Ab7|A#m|Abm|A#|Ab|Am|A|" +
        @"Bbmaj7|Bmaj7|Bbm7|Bm7|Bb7|B7|Bbm|Bm|Bb|B|" +
        @"C#maj7|Cmaj7|C#m7|Cm7|C#7|C7|C#m|Cm|C#|C|" +
        @"D#maj7|Dbmaj7|Dmaj7|D#m7|Dbm7|Dm7|D#7|Db7|D7|D#m|Dbm|Dm|D#|Db|D|" +
        @"Ebmaj7|Emaj7|Ebm7|Em7|Eb7|E7|Ebm|Em|Eb|E|" +
        @"F#maj7|Fmaj7|F#m7|Fm7|F#7|F7|F#m|Fm|F#|F|" +
        @"G#maj7|Gbmaj7|Gmaj7|G#m7|Gbm7|Gm7|G#7|Gb7|G7|G#m|Gbm|Gm|G#|Gb|G)" +
        @"(?![a-zA-Z])";

    public IHtmlContent RenderContent()
    {
        if (Song == null) return HtmlString.Empty;
        var sb = new StringBuilder();
        int last = 0;

        foreach (Match m in ChordRegex.Matches(Song.Content))
        {
            if (m.Index > last)
                sb.Append(HtmlEncoder.Default.Encode(Song.Content[last..m.Index]));

            var name = HtmlEncoder.Default.Encode(m.Value);
            sb.Append($"""<span class="chord" data-chord="{name}">{name}</span>""");
            last = m.Index + m.Length;
        }

        if (last < Song.Content.Length)
            sb.Append(HtmlEncoder.Default.Encode(Song.Content[last..]));

        return new HtmlString(sb.ToString());
    }

}
