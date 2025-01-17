using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Web;

namespace Security.Extensions.Services;

file class RestorePasswordToken {
    public string UserId { get; set; }
}

internal class RestorePasswordService {
    private readonly Type securityUserType;
    private readonly INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory;
    private readonly NavigationManager navigationManager;
    private readonly ITimeLimitedDataProtector dataProtector;
    public const string LoginProviderName = "RestorePasswordToken";

    public RestorePasswordService(
            IOptions<SecurityOptions> securityOptions,
            INonSecuredObjectSpaceFactory nonSecuredObjectSpaceFactory,
            NavigationManager navigationManager,
            IDataProtectionProvider dataProtectionProvider) {
        securityUserType = securityOptions.Value.UserType;
        this.nonSecuredObjectSpaceFactory = nonSecuredObjectSpaceFactory;
        this.navigationManager = navigationManager;
        dataProtector = dataProtectionProvider.CreateProtector("RestorePassword").ToTimeLimitedDataProtector();
    }

    public string GenerateRestorePasswordUrl(string userEmail) {
        if (string.IsNullOrWhiteSpace(userEmail)) {
            throw new UserFriendlyException("Email address is not specified.");
        }
        using var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace(securityUserType);
        var user = nonSecuredObjectSpace.FindObject(securityUserType, CriteriaOperator.Parse("Email = ?", userEmail)) as ISecurityUserWithLoginInfo;
        if (user == null) {
            throw new UserFriendlyException("Cannot find a user with the specified email address.");
        }
        var token = new RestorePasswordToken() {
            UserId = nonSecuredObjectSpace.GetKeyValueAsString(user)
        };
        var serializedToken = JsonSerializer.Serialize(token);
        var serializedProtectedToken = dataProtector.Protect(serializedToken, lifetime: TimeSpan.FromHours(1));
        // store the token in the database, replacing a previously issued token (if any)
        if (user.UserLogins.FirstOrDefault(info => info.LoginProviderName == LoginProviderName) is { } restorePasswordInfo) {
            nonSecuredObjectSpace.Delete(restorePasswordInfo);
        }
        user.CreateUserLoginInfo(LoginProviderName, serializedProtectedToken);
        nonSecuredObjectSpace.CommitChanges();
        return $"{navigationManager.BaseUri}LoginPage?restorePasswordToken={serializedProtectedToken}";
    }

    public void SetNewPassword(string restorePasswordToken, string newPassword) {
        var serializedUnprotectedToken = dataProtector.Unprotect(restorePasswordToken);
        var token = JsonSerializer.Deserialize<RestorePasswordToken>(serializedUnprotectedToken);
        using var nonSecuredObjectSpace = nonSecuredObjectSpaceFactory.CreateNonSecuredObjectSpace(securityUserType);
        var userKey = nonSecuredObjectSpace.GetObjectKey(securityUserType, token.UserId);
        var user = (ISecurityUserWithLoginInfo)nonSecuredObjectSpace.GetObjectByKey(securityUserType, userKey);
        var restorePasswordInfo = user.UserLogins.FirstOrDefault(info => info.LoginProviderName == LoginProviderName);
        // the processed token must match the token stored in the database
        if (restorePasswordInfo?.ProviderUserKey != restorePasswordToken) {
            throw new InvalidOperationException();
        }
        nonSecuredObjectSpace.Delete(restorePasswordInfo);
        ((IAuthenticationStandardUser)user).SetPassword(newPassword);
        nonSecuredObjectSpace.CommitChanges();
    }

    public string GetRestorePasswordTokenFromUrl() {
        var query = HttpUtility.ParseQueryString(new Uri(navigationManager.Uri).Query);
        return query["restorePasswordToken"];
    }
}
