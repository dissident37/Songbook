namespace Songbook.Web.Models;

public class Playlist
{
    public int Id { get; set; }
    public string Name { get; set; } = "";

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public List<PlaylistSong> PlaylistSongs { get; set; } = new();
}
