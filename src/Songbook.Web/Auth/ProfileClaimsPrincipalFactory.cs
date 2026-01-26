using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Songbook.Web.Data;
using Songbook.Web.Models;

namespace Songbook.Web.Auth;

/// <summary>
/// Erstellt Claims für den eingeloggten Benutzer.
/// Hier erzeugen wir automatisch ein Domain-Profil in der Tabelle "Users",
/// falls es noch nicht existiert, und speichern ProfileId als Claim.
/// </summary>
public class ProfileClaimsPrincipalFactory<TIdentityUser> : UserClaimsPrincipalFactory<TIdentityUser>
    where TIdentityUser : IdentityUser
{
    private readonly SongbookDbContext _db;

    public ProfileClaimsPrincipalFactory(
        UserManager<TIdentityUser> userManager,
        IOptions<IdentityOptions> optionsAccessor,
        SongbookDbContext db)
        : base(userManager, optionsAccessor)
    {
        _db = db;
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TIdentityUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Domain-Profil anhand AspNetUsers.Id finden
        var profile = await _db.Users
            .SingleOrDefaultAsync(u => u.IdentityUserId == user.Id);

        // Falls nicht vorhanden -> erstellen
        if (profile == null)
        {
            profile = new User
            {
                IdentityUserId = user.Id,
                DisplayName = user.UserName
            };

            _db.Users.Add(profile);
            await _db.SaveChangesAsync();
        }

        // ProfileId als Claim speichern
        identity.AddClaim(new Claim("ProfileId", profile.Id.ToString()));

        return identity;
    }
}
