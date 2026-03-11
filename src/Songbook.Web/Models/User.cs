namespace Songbook.Web.Models;

/// <summary>
/// Domänenmodell für das Benutzerprofil.
/// Dieses Modell hat NICHTS mit Login/Passwort zu tun.
/// </summary>
public class User
{
    /// <summary>
    /// Interne ID des Profils (Primary Key).
    /// Wird in Songs/Playlists als Foreign Key verwendet.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Verknüpfung zum ASP.NET Identity Benutzer.
    /// Entspricht AspNetUsers.Id (string).
    /// </summary>
    public string IdentityUserId { get; set; } = null!;

    /// <summary>
    /// Anzeigename im UI (optional).
    /// Hat nichts mit dem Login-Namen zu tun.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Alle Songs, die diesem Benutzer gehören.
    /// </summary>
    public List<Song> Songs { get; set; } = new();

    /// <summary>
    /// Alle Playlists dieses Benutzers.
    /// </summary>
    public List<Playlist> Playlists { get; set; } = new();
}
