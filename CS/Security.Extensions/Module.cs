using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base.Security;
using Microsoft.Extensions.DependencyInjection;
using Security.Extensions.Controllers;
using Security.Extensions.Services;
using System.Collections.Generic;

namespace Security.Extensions;

// A delegate that, when invoked, creates and returns a new user with the specified user name, password and email address.
public delegate IAuthenticationStandardUser CreateSecuritySystemUser(IObjectSpace objectSpace, string userName, string email, string password, bool isAdministrator);

public sealed class SecurityExtensionsModule : ModuleBase {
    public SecurityExtensionsModule() {
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Security.SecurityModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.ValidationModule));
    }

    public override void Setup(XafApplication application) {
        base.Setup(application);
        application.CreateCustomLogonWindowControllers += application_CreateCustomLogonWindowControllers;
        application.CreateCustomLogonAction += Application_CreateCustomLogonAction;
    }

    private void application_CreateCustomLogonWindowControllers(object sender, CreateCustomLogonWindowControllersEventArgs e) {
        var application = (XafApplication)sender;
        e.Controllers.Add(application.CreateController<ManageUsersOnLogonController>());
        e.Controllers.Add(application.CreateController<LogonActionCustomizationController>());
    }

    private void Application_CreateCustomLogonAction(object sender, CreateCustomLogonActionEventArgs e) {
        var restorePasswordService = Application.ServiceProvider.GetRequiredService<RestorePasswordService>();
        var restorePasswordToken = restorePasswordService.GetRestorePasswordTokenFromUrl();
        if (!string.IsNullOrEmpty(restorePasswordToken)) {
            var logonWindowControllers = e.CreateLogonWindowControllers();
            e.LogonAction = CreateRestorePasswordLogonAction(restorePasswordToken, logonWindowControllers);
        }
    }

    private PopupWindowShowAction CreateRestorePasswordLogonAction(string restorePasswordToken, List<Controller> logonWindowControllers) {
        var restorePasswordLogonAction = new PopupWindowShowAction();
        restorePasswordLogonAction.Application = Application;
        restorePasswordLogonAction.CustomizePopupWindowParams += (s, e) => {
            var objectSpace = Application.CreateObjectSpace<SetNewPasswordParameters>();
            var newPasswordParameters = objectSpace.CreateObject<SetNewPasswordParameters>();
            newPasswordParameters.Token = restorePasswordToken;
            e.View = Application.CreateDetailView(objectSpace, newPasswordParameters);
            e.DialogController = Application.CreateController<LogonController>();
            e.DialogController.Controllers.AddRange(logonWindowControllers);
        };
        return restorePasswordLogonAction;
    }
}
