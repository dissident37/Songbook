using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Songbook.Web.Models;

namespace Songbook.Web.Areas.Identity.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public LogoutModel(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public async Task<IActionResult> OnPost()
    {
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Index", new { area = "" });
    }

    public IActionResult OnGet()
    {
        return Page();
    }
}
