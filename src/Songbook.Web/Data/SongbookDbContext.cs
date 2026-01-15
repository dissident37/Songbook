using Microsoft.EntityFrameworkCore;
using Songbook.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Songbook.Web.Data;

public class SongbookDbContext : IdentityDbContext<User, IdentityRole<int>, int>

{
    public SongbookDbContext(DbContextOptions<SongbookDbContext> options)
        : base(options)
    {
    }

    public DbSet<Song> Songs => Set<Song>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Chord> Chords => Set<Chord>();
    public DbSet<SongChord> SongChords => Set<SongChord>();
    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<PlaylistSong> PlaylistSongs => Set<PlaylistSong>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("Users");
        // ---- Song ↔ Artist (1:N)
        modelBuilder.Entity<Song>()
            .HasOne(s => s.Artist)
            .WithMany(a => a.Songs)
            .HasForeignKey(s => s.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---- Song ↔ User (1:N)
        modelBuilder.Entity<Song>()
            .HasOne(s => s.CreatedByUser)
            .WithMany(u => u.Songs)
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Song ↔ Chord (M:N через SongChord)
        modelBuilder.Entity<SongChord>()
            .HasKey(sc => new { sc.SongId, sc.ChordId });

        modelBuilder.Entity<SongChord>()
            .HasOne(sc => sc.Song)
            .WithMany(s => s.SongChords)
            .HasForeignKey(sc => sc.SongId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SongChord>()
            .HasOne(sc => sc.Chord)
            .WithMany(c => c.SongChords)
            .HasForeignKey(sc => sc.ChordId);

        // ---- Playlist ↔ Song (M:N через PlaylistSong)
        modelBuilder.Entity<PlaylistSong>()
            .HasKey(ps => new { ps.PlaylistId, ps.SongId });

        modelBuilder.Entity<PlaylistSong>()
            .HasOne(ps => ps.Playlist)
            .WithMany(p => p.PlaylistSongs)
            .HasForeignKey(ps => ps.PlaylistId);

        modelBuilder.Entity<PlaylistSong>()
            .HasOne(ps => ps.Song)
            .WithMany(s => s.PlaylistSongs)
            .HasForeignKey(ps => ps.SongId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}
