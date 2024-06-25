<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/134075799/19.2.7%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/E4037)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->
<!-- default file list -->
*Files to look at*:

* [Updater.cs](./CS/ManageUsersOnLogon.Module/DatabaseUpdate/Updater.cs) (VB: [Updater.vb](./VB/ManageUsersOnLogon.Module/DatabaseUpdate/Updater.vb))
* [LogonActionParameters.cs](./CS/Security.Extensions/LogonActionParameters.cs) (VB: [LogonActionParameters.vb](./VB/Security.Extensions/LogonActionParameters.vb))
* [ManageUsersOnLogonController.cs](./CS/Security.Extensions/ManageUsersOnLogonController.cs) (VB: [ManageUsersOnLogonController.vb](./VB/Security.Extensions/ManageUsersOnLogonController.vb))
* [Module.cs](./CS/Security.Extensions/Module.cs) (VB: [Module.vb](./VB/Security.Extensions/Module.vb))
* [WebApplication.cs](./CS/ManageUsersOnLogon.Web/WebApplication.cs) (VB: [WebApplication.vb](./VB/ManageUsersOnLogon.Web/WebApplication.vb))
<!-- default file list end -->

# How to manage users (register a new user, restore a password, etc.) from the logon form in ASP.NET

> There is an alternative solution: <a href="https://www.devexpress.com/Support/Center/p/T535280">How to: Use Google, Facebook and Microsoft accounts in ASP.NET XAF applications (OAuth2 authentication demo)</a>. Instead of creating and maintaining a quite complex custom-tailored implementation for managing user authentication and registration from the logon form, we recommend delegating these routine tasks to OAuth2 providers. For instance, Microsoft or Google provide Microsoft 365 and GSuite services for managing users (e.g., register and delete users, reset forgotten passwords), documents, apps and other things within an organization using standard and familiar for business people means. Your XAF application will just smoothly integrate these OAuth2 providers into the logon form after adding some boilerplate code.
    
---------------------------------

## Scenario
This example contains a reusable `Security.Extensions` module that provides a possible solution for the following scenarios:
 - <a href="https://www.devexpress.com/Support/Center/p/S32938">Security - provide the capability to register a new user from the logon form</a>
 - <a href="https://www.devexpress.com/Support/Center/p/S33481">Security.Authentication - provide a "Forgot Password" feature</a>

![](https://raw.githubusercontent.com/DevExpress-Examples/obsolete-how-to-manage-users-register-a-new-user-restore-a-password-etc-from-the-logon-form-e4037/16.2.3+/media/08b47836-b8ac-11e6-80bf-00155d62480c.png)

---------------------------------

## Implementation Steps

In order to use this module in your project, do the following:
1. Download and include the `Security.Extensions` module project into your XAF solution (<a href="https://msdn.microsoft.com/library/ff460187.aspx">as per MSDN</a>) and rebuild it. This custom module contains Application Model settings (Model.DesignedDiffs.xafml) to layout custom Actions next to the logon form input fields (see the <a href="https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112816.aspx">How to: Include an Action to a Detail View Layout</a>  article for more details) as well as non-persistent data models for parameter screens (LogonActionParameters.cs) and finally a ViewController (ManageUsersOnLogonController.cs) for the logon DetailView that declares custom Actions and their behavior. The controller is registered via the XafApplication.<a href="https://documentation.devexpress.com/eXpressAppFramework/DevExpressExpressAppXafApplication_CreateCustomLogonWindowControllerstopic.aspx">CreateCustomLogonWindowControllers</a> event in the ModuleBase descendant (Module.cs) along with other service logic.
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
4. In the YourSolutionName.Web/WebApplication file, add the following code into the WebApplication constructor to customize the [SecurityStrategyComplex.AnonymousAllowedTypes](https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy.AnonymousAllowedTypes) property:
```cs
        public ManageUsersOnLogonAspNetApplication() {
            InitializeComponent();
            ((SecurityStrategy)Security).AnonymousAllowedTypes.Add(typeof(PermissionPolicyUser));
            ((SecurityStrategy)Security).AnonymousAllowedTypes.Add(typeof(PermissionPolicyRole));
        }
```

>This module is for ASP.NET only.
<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=XAF_logon-form-manage-users-register-a-new-user-restore-a-password&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=XAF_logon-form-manage-users-register-a-new-user-restore-a-password&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->
