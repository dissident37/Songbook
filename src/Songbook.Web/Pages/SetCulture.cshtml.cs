using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Songbook.Web.Pages;

public class SetCultureModel : PageModel
{
    private static readonly HashSet<string> SupportedCultures = new(StringComparer.OrdinalIgnoreCase)
    {
        "de",
        "en",
        "ru"
    };

    public IActionResult OnGet(string? culture, string? returnUrl)
    {
        var selectedCulture = SupportedCultures.Contains(culture ?? string.Empty) ? culture!.ToLowerInvariant() : "de";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(selectedCulture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

        var targetUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Page("/Index") ?? "/";

        return LocalRedirect(targetUrl);
    }
}
