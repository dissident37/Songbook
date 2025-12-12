namespace Songbook.Web.Models;

public class Chord
{
    public int Id { get; set; }
    public string Name { get; set; } = "";       // Am, F#m, G
    public string ImagePath { get; set; } = "";  // путь к картинке аккорда

    public List<SongChord> SongChords { get; set; } = new();
}
