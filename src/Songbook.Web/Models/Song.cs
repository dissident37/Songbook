namespace Songbook.Web.Models;

public class Song
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    public string Content { get; set; } = "";        // текст с аккордами
    public string ContentPlain { get; set; } = "";   // текст без аккордов

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    // связи M:N с аккордами
    public List<SongChord> SongChords { get; set; } = new();

    // связь с плейлистами (M:N)
    public List<PlaylistSong> PlaylistSongs { get; set; } = new();
}
