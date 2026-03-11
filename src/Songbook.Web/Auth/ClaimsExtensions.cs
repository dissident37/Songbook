using System.Security.Claims;

namespace Songbook.Web.Auth;

/// <summary>
/// Hilfsmethoden für den Zugriff auf Benutzerinformationen.
/// </summary>
public static class ClaimsExtensions
{
    /// <summary>
    /// Gibt die interne Profil-ID des aktuell eingeloggten Benutzers zurück.
    /// Diese ID stammt aus der Tabelle "Users".
    /// </summary>
    public static int GetProfileId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("ProfileId");

        return int.TryParse(value, out var id)
            ? id
            : 0;
    }
}
