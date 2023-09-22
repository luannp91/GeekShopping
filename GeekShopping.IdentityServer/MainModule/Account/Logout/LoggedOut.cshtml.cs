using Duende.IdentityServer.Services;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foo.Pages.Logout;

[SecurityHeaders]
[AllowAnonymous]
public class LoggedOut : PageModel
{
    private readonly IIdentityServerInteractionService _interactionService;
        
    public LoggedOutViewModel View { get; set; }

#pragma warning disable CS8618 // O campo n�o anul�vel precisa conter um valor n�o nulo ao sair do construtor. Considere declar�-lo como anul�vel.
    public LoggedOut(IIdentityServerInteractionService interactionService)
#pragma warning restore CS8618 // O campo n�o anul�vel precisa conter um valor n�o nulo ao sair do construtor. Considere declar�-lo como anul�vel.
    {
        _interactionService = interactionService;
    }

    public async Task OnGet(string logoutId)
    {
        // get context information (client name, post logout redirect URI and iframe for federated signout)
        var logout = await _interactionService.GetLogoutContextAsync(logoutId);

#pragma warning disable CS8601 // Poss�vel atribui��o de refer�ncia nula.
        View = new LoggedOutViewModel
        {
            AutomaticRedirectAfterSignOut = LogoutOptions.AutomaticRedirectAfterSignOut,
            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri,
            ClientName = String.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName,
            SignOutIframeUrl = logout?.SignOutIFrameUrl
        };
#pragma warning restore CS8601 // Poss�vel atribui��o de refer�ncia nula.
    }
}