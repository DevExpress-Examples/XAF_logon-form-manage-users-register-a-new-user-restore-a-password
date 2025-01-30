<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/134075799/24.2.1%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/E4037)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
[![](https://img.shields.io/badge/💬_Leave_Feedback-feecdd?style=flat-square)](#does-this-example-address-your-development-requirementsobjectives)
<!-- default badges end -->
<!-- default file list -->

# XAF Blazor UI: How to extend the logon form - register a new user, restore a password

> **NOTE:**
> An alternative solution exists: 
>
> [How to: Use Google, Facebook and Microsoft accounts in ASP.NET XAF applications (OAuth2 authentication demo)](https://github.com/DevExpress-Examples/xaf-web-forms-use-oauth2-authentication-providers). 
>
> Instead of a custom-tailored implementation, we recommend that you delegate these routine tasks to OAuth2 providers. Microsoft 365 or Google GSuite services enable user and document management that's familiar to anyone who works with business apps. Your XAF application can easily integrate these OAuth2 providers into the logon form. You only need to add some boilerplate code.
    
## Implementation Details

This example contains a reusable `Security.Extensions` module that enables the following functionality:

 - [Security - add a capability to register a new user from the logon form](https://supportcenter.devexpress.com/ticket/details/s32938/security-how-to-register-a-new-user-from-the-logon-form)
 - [Security.Authentication - add a "Forgot Password" feature](https://supportcenter.devexpress.com/ticket/details/s33481/security-authentication-provide-a-forgot-password-feature)

![XAF application Log In dialog](xaf-login-form.png)

The module includes the following notable building blocks:

- Non-persistent data models for parameter screens ([LogonActionParameters.cs](./CS/Security.Extensions/LogonActionParameters.cs)).
- A View Controller ([ManageUsersOnLogonController.cs](./CS/Security.Extensions/Controllers/ManageUsersOnLogonController.cs)) for the logon Detail View. The controller declares custom Actions and their behavior. See the [XafApplication.CreateCustomLogonWindowControllers](https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.XafApplication.CreateCustomLogonWindowControllers) event in [Module.cs](./CS/Security.Extensions/Module.cs) to find controller registration code and other service logic.
- Services for restoring password ([RestorePasswordService.cs](./CS/Security.Extensions/Services/RestorePasswordService.cs)) and registering new users ([UserRegistrationService.cs](./CS/Security.Extensions/Services/UserRegistrationService.cs)).
- A custom logon view for restoring a user's password (see the `Application_CreateCustomLogonAction` event handler in [Module.cs](./CS/Security.Extensions/Module.cs)).

---------------------------------

## Extend the Logon Form in Your XAF Blazor Project

In order to use this module in your own project, follow the steps below: 

1. Download the `Security.Extensions` module project, include it in your XAF solution, add a project reference to it in the Blazor project, and rebuild the solution. See [How to Add an Existing Project](https://learn.microsoft.com/en-us/previous-versions/ff460187(v=vs.140)) in MSDN for details.

2. Add the `Security.Extensions.SecurityExtensionsModule` to your application:

    **File:** _DXApplication1.Blazor.Server/Startup.cs_

    ```cs
    public class Startup {
        public void ConfigureServices(IServiceCollection services) {
            services.AddXaf(Configuration, builder => {
                builder.Modules
                    .AddSecurityExtensions(options => options.CreateSecuritySystemUser = DXApplication1.Module.DatabaseUpdate.Updater.CreateUser)
                    // ...
                // ...
            });
            // ...
        }
        // ...
    }
    ```

    In the above code sample, `Updater.CreateUser` is your custom method that matches the following definition:

    ```cs
    public delegate IAuthenticationStandardUser CreateSecuritySystemUser(IObjectSpace objectSpace, string userName, string email, string password, bool isAdministrator);
    ```

3. Add an `Email` property to the `ApplicationUser` class:

    **File:** _DXApplication1.Module/BusinessObjects/ApplicationUser.cs_

    ```cs
    // EF Core:
    public class ApplicationUser : /*...*/ {
        // ...
        public virtual string Email { get; set; }
    }
    // XPO:
    public class ApplicationUser : /*...*/ {
        // ...
        private string email;
        public string Email {
            get { return email; }
            set { SetPropertyValue(nameof(Email), ref email, value); }
        }
    }
    ```

4. (XPO only) Add the `Size` attribute to the `ApplicationUserLoginInfo.ProviderUserKey` property:

    **File:** _DXApplication1.Module/BusinessObjects/ApplicationUserLoginInfo.cs_

    ```cs
    // XPO:
    public class ApplicationUserLoginInfo : /*...*/ {
        // ...
        [Size(1024)]
        public string ProviderUserKey { /*...*/ }
    }
    ```

## Files to Review

EF Core:

* [Updater.cs](./CS/EFCore/DXApplication1.Module/DatabaseUpdate/Updater.cs)
* [Startup.cs](./CS/EFCore/DXApplication1.Blazor.Server/Startup.cs)

XPO:

* [Updater.cs](./CS/XPO/DXApplication1.Module/DatabaseUpdate/Updater.cs)
* [Startup.cs](./CS/XPO/DXApplication1.Blazor.Server/Startup.cs)

Common:

* [LogonActionCustomizationController.cs](./CS/Security.Extensions/Controllers/LogonActionCustomizationController.cs)
* [ManageUsersOnLogonController.cs](./CS/Security.Extensions/Controllers/ManageUsersOnLogonController.cs)
* [RestorePasswordService.cs](./CS/Security.Extensions/Services/RestorePasswordService.cs)
* [UserRegistrationService.cs](./CS/Security.Extensions/Services/UserRegistrationService.cs)
* [ApplicationBuilderExtensions.cs](./CS/Security.Extensions/ApplicationBuilderExtensions.cs)
* [LogonActionParameters.cs](./CS/Security.Extensions/LogonActionParameters.cs)
* [Module.cs](./CS/Security.Extensions/Module.cs)

<!-- feedback -->
## Does this example address your development requirements/objectives?

[<img src="https://www.devexpress.com/support/examples/i/yes-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=XAF_logon-form-manage-users-register-a-new-user-restore-a-password&~~~was_helpful=yes) [<img src="https://www.devexpress.com/support/examples/i/no-button.svg"/>](https://www.devexpress.com/support/examples/survey.xml?utm_source=github&utm_campaign=XAF_logon-form-manage-users-register-a-new-user-restore-a-password&~~~was_helpful=no)

(you will be redirected to DevExpress.com to submit your response)
<!-- feedback end -->