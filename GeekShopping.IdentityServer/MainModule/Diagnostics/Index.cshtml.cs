using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using IdentityServerHost.Quickstart.UI;

namespace Foo.Pages.Diagnostics;

[SecurityHeaders]
[Authorize]
public class Index : PageModel
{
    public ViewModel? View { get; set; }
        
    public async Task<IActionResult> OnGet()
    {
#pragma warning disable CS8602 // Desrefer�ncia de uma refer�ncia possivelmente nula.
        var localAddresses = new string[] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };
#pragma warning restore CS8602 // Desrefer�ncia de uma refer�ncia possivelmente nula.
#pragma warning disable CS8602 // Desrefer�ncia de uma refer�ncia possivelmente nula.
        if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
        {
            return NotFound();
        }
#pragma warning restore CS8602 // Desrefer�ncia de uma refer�ncia possivelmente nula.

        View = new ViewModel(await HttpContext.AuthenticateAsync());
            
        return Page();
    }
}