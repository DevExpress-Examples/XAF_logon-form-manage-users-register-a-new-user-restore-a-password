using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Templates;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Security.Extensions.Services;
using System;

namespace Security.Extensions.Controllers;

// A controller that adds the "Register"/"Restore password" buttons to the login page.
public class ManageUsersOnLogonController : ViewController<DetailView> {
    private const string LogonActionParametersActiveKey = "Active for LogonActionParameters only";
    public SimpleAction RegisterUserAction { get; }
    public SimpleAction RestorePasswordAction { get; }
    public SimpleAction CancelAction { get; }
    public SimpleAction AcceptLogonParametersAction { get; }

    public ManageUsersOnLogonController() {
        RegisterUserAction = new SimpleAction(this, "RegisterUser", PredefinedCategory.PopupActions) {
            Caption = "Register User",
            ToolTip = "Register",
            ImageName = "BO_User",
            PaintStyle = ActionItemPaintStyle.Image,
        };
        RegisterUserAction.Execute += (s, e) => ShowLogonActionView(typeof(RegisterUserParameters));

        RestorePasswordAction = new SimpleAction(this, "RestorePassword", PredefinedCategory.PopupActions) {
            Caption = "Restore Password",
            ToolTip = "Restore Password",
            ImageName = "Action_ResetPassword",
            PaintStyle = ActionItemPaintStyle.Image,
        };
        RestorePasswordAction.Execute += (s, e) => ShowLogonActionView(typeof(RestorePasswordParameters));

        AcceptLogonParametersAction = new SimpleAction(this, "AcceptLogonParameters", PredefinedCategory.PopupActions) {
            Caption = "OK"
        };
        AcceptLogonParametersAction.Execute += (s, e) => AcceptParameters(e.CurrentObject as LogonActionParametersBase);

        CancelAction = new SimpleAction(this, "CancelLogonParameters", PredefinedCategory.PopupActions) {
            Caption = "Cancel"
        };
        CancelAction.Execute += (s, e) => ReloadPage();
    }

    // Ensures that this controller is active only when a user is not logged on.
    protected override void OnFrameAssigned() {
        base.OnFrameAssigned();
        Active[ControllerActiveKey] = !Application.Security.IsAuthenticated;
    }

    // Manages the activity of Actions within the logon window depending on the current context.
    protected override void OnViewControlsCreated() {
        base.OnViewControlsCreated();
        // Manage the state of own Actions as well as dialog Actions of the LogonController class within the same logon Frame.
        bool isRegisterUserOrRestorePasswordView = View?.ObjectTypeInfo?.Implements<LogonActionParametersBase>() ?? false;
        LogonController logonController = Frame.GetController<LogonController>();
        if (logonController != null) {
            logonController.AcceptAction.Active[LogonActionParametersActiveKey] = !isRegisterUserOrRestorePasswordView;
            logonController.CancelAction.Active[LogonActionParametersActiveKey] = !isRegisterUserOrRestorePasswordView;
        }
        AcceptLogonParametersAction.Active[LogonActionParametersActiveKey] = isRegisterUserOrRestorePasswordView;
        CancelAction.Active[LogonActionParametersActiveKey] = isRegisterUserOrRestorePasswordView;
        RegisterUserAction.Active[LogonActionParametersActiveKey] = !isRegisterUserOrRestorePasswordView;
        RestorePasswordAction.Active[LogonActionParametersActiveKey] = !isRegisterUserOrRestorePasswordView;
    }

    // Configures a View used to display our parameters objects. 
    private void ShowLogonActionView(Type logonActionParametersType) {
        ArgumentNullException.ThrowIfNull(logonActionParametersType);
        var objectSpace = Application.CreateObjectSpace(logonActionParametersType);
        var logonActionParameters = objectSpace.CreateObject(logonActionParametersType);
        var detailView = Application.CreateDetailView(objectSpace, logonActionParameters);
        Frame.SetView(detailView);
    }

    private void AcceptParameters(LogonActionParametersBase logonActionParameters) {
        ArgumentNullException.ThrowIfNull(logonActionParameters);
        if (logonActionParameters is RegisterUserParameters registerUserParameters) {
            RegisterUser(registerUserParameters);
        }
        else if (logonActionParameters is RestorePasswordParameters restorePasswordParameters) {
            EmailRestorePasswordDetails(restorePasswordParameters);
        }
        else if (logonActionParameters is SetNewPasswordParameters setNewPasswordParameters) {
            SetNewPassword(setNewPasswordParameters);
        }
    }

    private void RegisterUser(RegisterUserParameters parameters) {
        var userRegistrationService = Application.ServiceProvider.GetRequiredService<UserRegistrationService>();
        userRegistrationService.RegisterNewUserAndLogin(parameters.UserName, parameters.Email, parameters.Password, isAdministrator: false);
    }

    private void EmailRestorePasswordDetails(RestorePasswordParameters parameters) {
        var restorePasswordService = Application.ServiceProvider.GetRequiredService<RestorePasswordService>();
        string restorePasswordUrl = restorePasswordService.GenerateRestorePasswordUrl(parameters.Email);
        // Send an email with the login details here.
        // Refer to https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.mailmessage for more details.
#if DEBUG
        // Display a notification with the reset password URL, close automatically after 5 minutes
        Application.ShowViewStrategy.ShowMessage(
            $"""
            (Debug mode) Follow the link below to open the password reset form:
            {restorePasswordUrl}
            """,
            InformationType.Info, displayInterval: 300_000);
#endif
    }

    private void SetNewPassword(SetNewPasswordParameters setNewPasswordParameters) {
        try {
            var restorePasswordService = Application.ServiceProvider.GetRequiredService<RestorePasswordService>();
            restorePasswordService.SetNewPassword(setNewPasswordParameters.Token, setNewPasswordParameters.Password);
            ReloadPage();
        }
        catch {
            throw new UserFriendlyException("The password reset link is invalid or has expired.");
        }
    }

    private void ReloadPage() {
        var navigationManager = Application.ServiceProvider.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo("LoginPage", forceLoad: true);
    }
}