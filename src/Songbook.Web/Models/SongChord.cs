namespace Songbook.Web.Models;

public class SongChord
{
    public int SongId { get; set; }
    public Song Song { get; set; } = null!;

    public int ChordId { get; set; }
    public Chord Chord { get; set; } = null!;
}
