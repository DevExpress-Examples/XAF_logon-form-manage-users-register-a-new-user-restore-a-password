<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/134075799/21.1.4%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/E4037)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
<!-- default file list -->

# How to manage users (register a new user, restore a password, etc.) from the logon form in XAF Blazor UI

> **Note**:
> There is an alternative solution: 
>
> [How to: Use Google, Facebook and Microsoft accounts in ASP.NET XAF applications (OAuth2 authentication demo)](https://github.com/DevExpress-Examples/xaf-web-forms-use-oauth2-authentication-providers). 
>
> Instead of creating and maintaining a quite complex custom-tailored implementation for managing user authentication and registration from the logon form, we recommend delegating these routine tasks to OAuth2 providers. For instance, Microsoft or Google provide Microsoft 365 and GSuite services for managing users (e.g., register and delete users, reset forgotten passwords), documents, apps and other things within an organization using standard and familiar for business people means. Your XAF application will just smoothly integrate these OAuth2 providers into the logon form after adding some boilerplate code.
    
## Scenario
This example contains a reusable `Security.Extensions` module that provides a possible solution for the following scenarios:
 - [Security - provide the capability to register a new user from the logon form](https://supportcenter.devexpress.com/ticket/details/s32938/security-how-to-register-a-new-user-from-the-logon-form)
 - [Security.Authentication - provide a "Forgot Password" feature](https://supportcenter.devexpress.com/ticket/details/s33481/security-authentication-provide-a-forgot-password-feature)

![image](https://user-images.githubusercontent.com/14300209/128016215-31fc417a-cfb9-4ce4-910a-e1e215c1c63d.png)


---------------------------------

## Implementation Steps

In order to use this module in your project, do the following: 

1. Download and include the `Security.Extensions` module project into your XAF solution ([as per MSDN](https://learn.microsoft.com/en-us/previous-versions/ff460187(v=vs.140)?redirectedfrom=MSDN)) and rebuild it. This custom module contains Application Model settings (Model.DesignedDiffs.xafml) to layout custom Actions next to the logon form input fields (see the [How to: Include an Action to a Detail View Layout](https://docs.devexpress.com/eXpressAppFramework/112816/ui-construction/view-items-and-property-editors/include-an-action-to-a-detail-view-layout)  article for more details) as well as non-persistent data models for parameter screens (LogonActionParameters.cs) and finally a ViewController (ManageUsersOnLogonController.cs) for the logon DetailView that declares custom Actions and their behavior. The controller is registered via the XafApplication.[CreateCustomLogonWindowControllers](https://documentation.devexpress.com/eXpressAppFramework/DevExpressExpressAppXafApplication_CreateCustomLogonWindowControllerstopic.aspx) event in the ModuleBase descendant (Module.cs) along with other service logic. 

2. Invoke the Module Designer for your platform-agnostic module and drag and drop the `SecurityExtensionsModule` from the Toolbox; 

3. Add the following code into your platform-agnostic module class:

   ```cs
   static YourPlatformAgnosticModuleName() {
       SecurityExtensionsModule.CreateSecuritySystemUser = Updater.CreateUser;
   } 
   ```
   where 'Updater.CreateUser' is your custom method that matches the following definition:
   ```cs
   public delegate IAuthenticationStandardUser CreateSecuritySystemUser(IObjectSpace objectSpace, string userName, string email, string password, bool isAdministrator);
   ```
4. In the YourSolutionName.Blazor.Server/Startup.cs file, add the following code into the ConfigureServices method to customize the [SecurityStrategyComplex.AnonymousAllowedTypes](https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy.AnonymousAllowedTypes) property:
   ```cs
   services.AddXafSecurity(options => {
       options.RoleType = typeof(PermissionPolicyRole);
       // ApplicationUser descends from PermissionPolicyUser and supports OAuth authentication. For more information, refer to the following help topic: https://docs.devexpress.com/eXpressAppFramework/402197
       // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
       options.UserType = typeof(DXApplication1.Module.BusinessObjects.ApplicationUser);

       // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
       // Comment out the following line if using PermissionPolicyUser or a custom user type.
       options.UserLoginInfoType = typeof(DXApplication1.Module.BusinessObjects.ApplicationUserLoginInfo);
       options.Events.OnSecurityStrategyCreated = securityStrategy => {
           ((SecurityStrategy)securityStrategy).RegisterXPOAdapterProviders();
           ((SecurityStrategy)securityStrategy).AnonymousAllowedTypes.Add(typeof(ApplicationUser));
           ((SecurityStrategy)securityStrategy).AnonymousAllowedTypes.Add(typeof(PermissionPolicyRole));
           ((SecurityStrategy)securityStrategy).AnonymousAllowedTypes.Add(typeof(ApplicationUserLoginInfo));
   };
   ```
   
## Files to Review

* [Updater.cs](./CS/DXApplication1.Module/DatabaseUpdate/Updater.cs)
* [LogonActionParameters.cs](./CS/Security.Extensions/LogonActionParameters.cs)
* [ManageUsersOnLogonController.cs](./CS/Security.Extensions/ManageUsersOnLogonController.cs) 
* [Module.cs](./CS/Security.Extensions/Module.cs)
* [Startup.cs](./CS/DXApplication1.Blazor.Server/Startup.cs)
