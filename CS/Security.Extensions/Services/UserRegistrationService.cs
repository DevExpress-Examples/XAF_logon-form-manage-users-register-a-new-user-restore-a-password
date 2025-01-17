using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System;

namespace Security.Extensions.Services;

internal class UserRegistrationService {
    private readonly UserManager userManager;
    private readonly Type securityUserType;
    private readonly SecurityExtensionsOptions moduleOptions;
    private readonly INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory;
    private readonly NavigationManager navigationManager;

    public UserRegistrationService(
            UserManager userManager,
            SignInManager signInManager,
            IOptions<SecurityOptions> securityOptions,
            IOptions<SecurityExtensionsOptions> moduleOptions,
            INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory,
            NavigationManager navigationManager) {
        this.userManager = userManager;
        this.securityUserType = securityOptions.Value.UserType;
        this.moduleOptions = moduleOptions.Value;
        this.nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
        this.navigationManager = navigationManager;
    }

    public void RegisterNewUserAndLogin(string userName, string email, string password, bool isAdministrator) {
        var newUser = RegisterNewUser(userName, email, password, isAdministrator);
        var authToken = userManager.GetAuthenticationToken((ISecurityUserWithLoginInfo)newUser, expirationSeconds: 15);
        var loginUrl = $"{SignInMiddlewareDefaults.SignInEndpointName}?token={authToken}";
        navigationManager.NavigateTo(loginUrl, true);
    }

    public IAuthenticationStandardUser RegisterNewUser(string userName, string email, string password, bool isAdministrator) {
        using var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace(securityUserType);
        var newUser = moduleOptions.CreateSecuritySystemUser(nonSecuredObjectSpace, userName, email, password, isAdministrator);
        return newUser;
    }
}
