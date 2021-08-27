<!-- default badges list -->
![](https://img.shields.io/endpoint?url=https://codecentral.devexpress.com/api/v1/VersionRange/134075799/15.1.3%2B)
[![](https://img.shields.io/badge/Open_in_DevExpress_Support_Center-FF7200?style=flat-square&logo=DevExpress&logoColor=white)](https://supportcenter.devexpress.com/ticket/details/E4037)
[![](https://img.shields.io/badge/📖_How_to_use_DevExpress_Examples-e9f6fc?style=flat-square)](https://docs.devexpress.com/GeneralInformation/403183)
<!-- default badges end -->
<!-- default file list -->
*Files to look at*:

* [Updater.cs](./CS/ManageUsersOnLogon.Module/DatabaseUpdate/Updater.cs) (VB: [Updater.vb](./VB/ManageUsersOnLogon.Module/DatabaseUpdate/Updater.vb))
* [E4037.ets](./CS/ManageUsersOnLogon.Module/FunctionalTests/E4037.ets) (VB: [E4037.ets](./VB/ManageUsersOnLogon.Module/FunctionalTests/E4037.ets))
* [LogonActionParameters.cs](./CS/Security.Extensions/LogonActionParameters.cs) (VB: [LogonActionParameters.vb](./VB/Security.Extensions/LogonActionParameters.vb))
* [ManageUsersOnLogonController.cs](./CS/Security.Extensions/ManageUsersOnLogonController.cs) (VB: [ManageUsersOnLogonController.vb](./VB/Security.Extensions/ManageUsersOnLogonController.vb))
* [Model.DesignedDiffs.xafml](./CS/Security.Extensions/Model.DesignedDiffs.xafml) (VB: [Model.DesignedDiffs.xafml](./VB/Security.Extensions/Model.DesignedDiffs.xafml))
* [Module.cs](./CS/Security.Extensions/Module.cs) (VB: [Module.vb](./VB/Security.Extensions/Module.vb))
<!-- default file list end -->
# OBSOLETE - How to manage users (register a new user, restore a password, etc.) from the logon form in ASP.NET


<p>We'd like to announce that we've published an example demonstrating an alternative and more recommended solution to managing user authentication, registration and related tasks:<br><a href="https://www.devexpress.com/Support/Center/p/T535280">How to: Use Google, Facebook and Microsoft accounts in ASP.NET XAF applications (OAuth2 authentication demo)</a><br><br>Instead of creating and maintaining a quite complex custom-tailored implementation for managing users from the logon form, we recommend delegating these routine tasks to OAuth2 providers. For instance, Microsoft or Google provide Office 365 and G Suite services for managing users (e.g., register and delete users, reset forgotten passwords), documents, apps and other things within an organization using standard and familiar for business people means. Your XAF application will just smoothly integrate these OAuth2 providers into the logon form after adding some boilerplate code.<br><br>Your feedback on this implementation and the approach in general is welcome.<strong><br>====================<br>Scenario</strong><br>This example contains a reusable <em>Security.Extensions</em> module that provides a possible solution for the following scenarios:</p>
<p><a href="https://www.devexpress.com/Support/Center/p/S32938">Security - provide the capability to register a new user from the logon form</a><br><a href="https://www.devexpress.com/Support/Center/p/S33481">Security.Authentication - provide a "Forgot Password" feature</a><br><br><img src="https://raw.githubusercontent.com/DevExpress-Examples/obsolete-how-to-manage-users-register-a-new-user-restore-a-password-etc-from-the-logon-form-e4037/15.1.3+/media/08b47836-b8ac-11e6-80bf-00155d62480c.png"></p>
<br><strong>Steps to implement</strong><br>
<p>In order to use this module in your project, do the following:</p>
<p>1. Download and include the Security.Extensions module project into your XAF solution (<a href="https://msdn.microsoft.com/library/ff460187.aspx">as per MSDN</a>) and rebuild it. This custom module contains Application Model settings (Model.DesignedDiffs.xafml) to layout custom Actions next to the logon form input fields (see the <a href="https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112816.aspx">How to: Include an Action to a Detail View Layout</a>  article for more details) as well as non-persistent data models for parameter screens (LogonActionParameters.cs) and finally a ViewController (ManageUsersOnLogonController.cs) for the logon DetailView that declares custom Actions and their behavior. The controller is registered via the XafApplication.<a href="https://documentation.devexpress.com/eXpressAppFramework/DevExpressExpressAppXafApplication_CreateCustomLogonWindowControllerstopic.aspx">CreateCustomLogonWindowControllers</a> event in the ModuleBase descendant (Module.cs) along with other service logic.</p>
<p>2. Invoke the Module Designer for your platform-agnostic module and drag and drop the <em>SecurityExtensionsModule </em>from the Toolbox;</p>
<p>3. Add the following code into your platform-agnostic module class:</p>


```cs
static YourPlatformAgnosticModuleName() {
    SecurityExtensionsModule.CreateSecuritySystemUser = Updater.CreateUser;
} 

```


<p>where <em>'Updater.CreateUser'</em> is your custom method that matches the following definition:</p>


```cs
public delegate IAuthenticationStandardUser CreateSecuritySystemUser(IObjectSpace objectSpace, string userName, string email, string password, bool isAdministrator);

```


<p> </p>
<p><strong>IM</strong><strong>PORTANT NOTE</strong><br> This module is currently ASP.NET only.<br><br></p>

<br/>


