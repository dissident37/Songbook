using System.ComponentModel.DataAnnotations;

namespace Songbook.Web.Models;

public class Song
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Artist { get; set; }

    [Required]
    public string Lyrics { get; set; } = string.Empty;

    public string? Chords { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
