namespace Songbook.Web.Models;

public class Song
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    public string Content { get; set; } = "";        // текст с аккордами
    public string ContentPlain { get; set; } = "";   // текст без аккордов

    // Sichtbarkeit durch den Besitzer:
    // - true  => öffentlich (für alle sichtbar)
    // - false => privat (nur für den Besitzer)
    public bool IsPublic { get; set; } = false;

    // Moderation (Admin):
    // Wenn true, wird das Lied für alle ausgeblendet – außer für den Besitzer
    // (damit der Besitzer nicht "Datenverlust" erlebt, sondern sieht, was passiert ist).
    public bool IsHiddenByAdmin { get; set; } = false;

    // Optional: kurzer Grund für die Ausblendung (z.B. Spam, Beleidigung, Werbung).
    // Kann im UI später angezeigt werden (nur für Besitzer/Admin).
    public string? HiddenReason { get; set; }

    // Ownership (Domain User):
    // Wer das Lied erstellt hat. Ist die Basis für "nur der Besitzer darf editieren/löschen".
    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    // Beziehungen (M:N) mit Akkorden
    public List<SongChord> SongChords { get; set; } = new();

    // Beziehung zu Playlists (M:N)
    public List<PlaylistSong> PlaylistSongs { get; set; } = new();
}
