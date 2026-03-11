using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Songbook.Web.Auth;
using Songbook.Web.Data;
using Songbook.Web.Models;


var builder = WebApplication.CreateBuilder(args);

// ===== Services (Dienste registrieren) =====

// Localization (nur UI-Texte)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services
    .AddRazorPages()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var supportedCultures = new[]
{
    new CultureInfo("de"),
    new CultureInfo("en"),
    new CultureInfo("ru")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("de");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider
    {
        QueryStringKey = "culture",
        UIQueryStringKey = "culture"
    });
});

// Autorisierung: Admin-Bereich (Rolle wird per Seed erzeugt)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// Datenbank für App-Daten (Songs, Artists, Playlists, Domain-Userprofil)
builder.Services.AddDbContext<SongbookDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Datenbank für Auth (ASP.NET Identity Tabellen: AspNetUsers, AspNetRoles, ...)
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity (Registrierung / Login) + Rollen aktivieren
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
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

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// Statische Dateien (CSS/JS)
app.UseStaticFiles();

// Routing
app.UseRouting();

// Authentifizierung & Autorisierung (wichtig für [Authorize])
app.UseAuthentication();
app.UseAuthorization();

// Identity-Rollen und Admin-User initialisieren (vor dem ersten Request)
await IdentitySeed.SeedAsync(app.Services);

// Razor Pages Endpunkte
app.MapRazorPages();

app.Run();
