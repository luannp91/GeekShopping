using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foo.Pages.Logout;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;

    [BindProperty] 
    public string LogoutId { get; set; }

#pragma warning disable CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere declará-lo como anulável.
    public Index(IIdentityServerInteractionService interaction, IEventService events)
#pragma warning restore CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere declará-lo como anulável.
    {
        _interaction = interaction;
        _events = events;
    }

    public async Task<IActionResult> OnGet(string logoutId)
    {
        LogoutId = logoutId;

        var showLogoutPrompt = LogoutOptions.ShowLogoutPrompt;

#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
        if (User?.Identity.IsAuthenticated != true)
        {
            // if the user is not authenticated, then just show logged out page
            showLogoutPrompt = false;
        }
        else
        {
            var context = await _interaction.GetLogoutContextAsync(LogoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                // it's safe to automatically sign-out
                showLogoutPrompt = false;
            }
        }
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.

        if (showLogoutPrompt == false)
        {
            // if the request for logout was properly authenticated from IdentityServer, then
            // we don't need to show the prompt and can just log the user out directly.
            return await OnPost();
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
        if (User?.Identity.IsAuthenticated == true)
        {
            // if there's no current logout context, we need to create one
            // this captures necessary info from the current logged in user
            // this can still return null if there is no context needed
#pragma warning disable CS8601 // Possível atribuição de referência nula.
            LogoutId ??= await _interaction.CreateLogoutContextAsync();
#pragma warning restore CS8601 // Possível atribuição de referência nula.

            // delete local authentication cookie
            await HttpContext.SignOutAsync();

            // raise the logout event
            await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));

            // see if we need to trigger federated logout
            var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;

            // if it's a local login we can ignore this workflow
            if (idp != null && idp != Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider)
            {
                // we need to see if the provider supports external logout
#pragma warning disable CS0618 // O tipo ou membro é obsoleto
                if (await HttpContext.GetSchemeSupportsSignOutAsync(idp))
                {
                    // build a return URL so the upstream provider will redirect back
                    // to us after the user has logged out. this allows us to then
                    // complete our single sign-out processing.
#pragma warning disable CS8600 // Conversão de literal nula ou possível valor nulo em tipo não anulável.
                    string url = Url.Page("/Account/Logout/Loggedout", new { logoutId = LogoutId });
#pragma warning restore CS8600 // Conversão de literal nula ou possível valor nulo em tipo não anulável.

                    // this triggers a redirect to the external provider for sign-out
                    return SignOut(new AuthenticationProperties { RedirectUri = url }, idp);
                }
#pragma warning restore CS0618 // O tipo ou membro é obsoleto
            }
        }
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.

        return RedirectToPage("/Account/Logout/LoggedOut", new { logoutId = LogoutId });
    }
}