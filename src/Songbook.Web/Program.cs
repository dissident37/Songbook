using Microsoft.EntityFrameworkCore;
using Songbook.Web.Data;
using Songbook.Web.Models;
using Songbook.Web.Auth;
using Microsoft.AspNetCore.Identity;


var builder = WebApplication.CreateBuilder(args);

// ===== Services (Dienste registrieren) =====
// Razor Pages
builder.Services.AddRazorPages();

// Datenbank für deine App-Daten (Songs, Artists, Playlists, Users-Profil)
builder.Services.AddDbContext<SongbookDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Datenbank für Auth (ASP.NET Identity Tabellen: AspNetUsers, AspNetRoles, ...)
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity (Registrierung / Login)
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AuthDbContext>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ProfileClaimsPrincipalFactory<ApplicationUser>>();


var app = builder.Build();

// ===== Pipeline (Reihenfolge der Middleware) =====
// Fehlerbehandlung
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Statische Dateien (CSS/JS)
app.UseStaticFiles();

// Routing
app.UseRouting();

// Authentifizierung & Autorisierung (wichtig für [Authorize])
app.UseAuthentication();
app.UseAuthorization();

// Razor Pages Endpunkte
app.MapRazorPages();

app.Run();
