using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages
builder.Services.AddRazorPages();

// DB Context
builder.Services.AddDbContext<SongbookDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files (CSS/JS)
app.UseStaticFiles();

// Routing
app.UseRouting();

// Authorization (not used yet, но пусть будет)
app.UseAuthorization();

// Это ГЛАВНАЯ СТРОКА:
app.MapRazorPages();

app.Run();
