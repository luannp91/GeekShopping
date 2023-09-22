using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Validation;
using IdentityModel;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Foo.Pages.Consent;

[Authorize]
[SecurityHeadersAttribute]
public class Index : PageModel
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly ILogger<Index> _logger;

#pragma warning disable CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere declará-lo como anulável.
    public Index(
#pragma warning restore CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere declará-lo como anulável.
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<Index> logger)
    {
        _interaction = interaction;
        _events = events;
        _logger = logger;
    }

    public ViewModel View { get; set; }
        
    [BindProperty]
    public InputModel Input { get; set; }

    public async Task<IActionResult> OnGet(string returnUrl)
    {
        View = await BuildViewModelAsync(returnUrl);
        if (View == null)
        {
            return RedirectToPage("/Home/Error/Index");
        }

        Input = new InputModel
        {
            ReturnUrl = returnUrl,
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // validate return url is still valid
        var request = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);
        if (request == null) return RedirectToPage("/Home/Error/Index");

        ConsentResponse? grantedConsent = null;

        // user clicked 'no' - send back the standard 'access_denied' response
        if (Input?.Button == "no")
        {
            grantedConsent = new ConsentResponse { Error = AuthorizationError.AccessDenied };

            // emit event
            await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues));
        }
        // user clicked 'yes' - validate the data
        else if (Input?.Button == "yes")
        {
            // if the user consented to some scope, build the response model
            if (Input.ScopesConsented != null && Input.ScopesConsented.Any())
            {
                var scopes = Input.ScopesConsented;
                if (ConsentOptions.EnableOfflineAccess == false)
                {
                    scopes = scopes.Where(x => x != Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess);
                }

                grantedConsent = new ConsentResponse
                {
                    RememberConsent = Input.RememberConsent,
                    ScopesValuesConsented = scopes.ToArray(),
                    Description = Input.Description
                };

                // emit event
                await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.Client.ClientId, request.ValidatedResources.RawScopeValues, grantedConsent.ScopesValuesConsented, grantedConsent.RememberConsent));
            }
            else
            {
                ModelState.AddModelError("", ConsentOptions.MustChooseOneErrorMessage);
            }
        }
        else
        {
            ModelState.AddModelError("", ConsentOptions.InvalidSelectionErrorMessage);
        }

        if (grantedConsent != null)
        {
            // communicate outcome of consent back to identityserver
            await _interaction.GrantConsentAsync(request, grantedConsent);

            // redirect back to authorization endpoint
            if (request.IsNativeClient() == true)
            {
                // The client is native, so this change in how to
                // return the response is for better UX for the end user.
#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
                return this.LoadingPage(Input.ReturnUrl);
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.
            }

#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
            return Redirect(Input.ReturnUrl);
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.
        }

        // we need to redisplay the consent UI
#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
        View = await BuildViewModelAsync(Input.ReturnUrl, Input);
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.
        return Page();
    }

    private IActionResult LoadingPage(string returnUrl)
    {
        throw new NotImplementedException();
    }

    private async Task<ViewModel> BuildViewModelAsync(string returnUrl, InputModel? model = null)
    {
        var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (request != null)
        {
#pragma warning disable CS8604 // Possível argumento de referência nula.
            return CreateConsentViewModel(model, returnUrl, request);
#pragma warning restore CS8604 // Possível argumento de referência nula.
        }
        else
        {
            _logger.LogError("No consent request matching request: {0}", returnUrl);
        }
#pragma warning disable CS8603 // Possível retorno de referência nula.
        return null;
#pragma warning restore CS8603 // Possível retorno de referência nula.
    }

    private ViewModel CreateConsentViewModel(
        InputModel model, string returnUrl,
        AuthorizationRequest request)
    {
#pragma warning disable CS8601 // Possível atribuição de referência nula.
        var vm = new ViewModel
        {
            ClientName = request.Client.ClientName ?? request.Client.ClientId,
            ClientUrl = request.Client.ClientUri,
            ClientLogoUrl = request.Client.LogoUri,
            AllowRememberConsent = request.Client.AllowRememberConsent
        };
#pragma warning restore CS8601 // Possível atribuição de referência nula.

        vm.IdentityScopes = request.ValidatedResources.Resources.IdentityResources
            .Select(x => CreateScopeViewModel(x, model?.ScopesConsented == null || model.ScopesConsented?.Contains(x.Name) == true))
            .ToArray();

        var resourceIndicators = request.Parameters.GetValues(OidcConstants.AuthorizeRequest.Resource) ?? Enumerable.Empty<string>();
        var apiResources = request.ValidatedResources.Resources.ApiResources.Where(x => resourceIndicators.Contains(x.Name));

        var apiScopes = new List<ScopeViewModel>();
        foreach (var parsedScope in request.ValidatedResources.ParsedScopes)
        {
            var apiScope = request.ValidatedResources.Resources.FindApiScope(parsedScope.ParsedName);
            if (apiScope != null)
            {
                var scopeVm = CreateScopeViewModel(parsedScope, apiScope, model == null || model.ScopesConsented?.Contains(parsedScope.RawValue) == true);
                scopeVm.Resources = apiResources.Where(x => x.Scopes.Contains(parsedScope.ParsedName))
                    .Select(x => new ResourceViewModel
                    {
                        Name = x.Name,
                        DisplayName = x.DisplayName ?? x.Name,
                    }).ToArray();
                apiScopes.Add(scopeVm);
            }
        }
        if (ConsentOptions.EnableOfflineAccess && request.ValidatedResources.Resources.OfflineAccess)
        {
            apiScopes.Add(GetOfflineAccessScope(model == null || model.ScopesConsented?.Contains(Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess) == true));
        }
        vm.ApiScopes = apiScopes;

        return vm;
    }

    private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
    {
#pragma warning disable CS8601 // Possível atribuição de referência nula.
        return new ScopeViewModel
        {
            Name = identity.Name,
            Value = identity.Name,
            DisplayName = identity.DisplayName ?? identity.Name,
            Description = identity.Description,
            Emphasize = identity.Emphasize,
            Required = identity.Required,
            Checked = check || identity.Required
        };
#pragma warning restore CS8601 // Possível atribuição de referência nula.
    }

    public ScopeViewModel CreateScopeViewModel(ParsedScopeValue parsedScopeValue, ApiScope apiScope, bool check)
    {
        var displayName = apiScope.DisplayName ?? apiScope.Name;
        if (!String.IsNullOrWhiteSpace(parsedScopeValue.ParsedParameter))
        {
            displayName += ":" + parsedScopeValue.ParsedParameter;
        }

#pragma warning disable CS8601 // Possível atribuição de referência nula.
        return new ScopeViewModel
        {
            Name = parsedScopeValue.ParsedName,
            Value = parsedScopeValue.RawValue,
            DisplayName = displayName,
            Description = apiScope.Description,
            Emphasize = apiScope.Emphasize,
            Required = apiScope.Required,
            Checked = check || apiScope.Required
        };
#pragma warning restore CS8601 // Possível atribuição de referência nula.
    }

    private ScopeViewModel GetOfflineAccessScope(bool check)
    {
        return new ScopeViewModel
        {
            Value = Duende.IdentityServer.IdentityServerConstants.StandardScopes.OfflineAccess,
            DisplayName = ConsentOptions.OfflineAccessDisplayName,
            Description = ConsentOptions.OfflineAccessDescription,
            Emphasize = true,
            Checked = check
        };
    }
}