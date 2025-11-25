using Microsoft.EntityFrameworkCore;
using Songbook.Web.Models;

namespace Songbook.Web.Data;

public class SongbookDbContext : DbContext
{
    public SongbookDbContext(DbContextOptions<SongbookDbContext> options)
        : base(options)
    {
    }

    public DbSet<Song> Songs => Set<Song>();
}
