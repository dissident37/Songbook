using Microsoft.AspNetCore.Identity;

namespace Songbook.Web.Models;

public class User : IdentityUser<int>
{
    public int Id { get; set; }

    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    public List<Song> Songs { get; set; } = new();
    public List<Playlist> Playlists { get; set; } = new();
}
