using DevExpress.Blazor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Templates.Toolbar.ActionControls;
using DevExpress.ExpressApp.SystemModule;

namespace Security.Extensions.Controllers;

// A controller that highlights the accept button ("OK") and allows submitting the form by pressing the Enter key.
public class LogonActionCustomizationController : WindowController {
    private ActionControlsSiteController actionControlsSiteController;

    private void ActionControlsSiteController_CustomizeActionControl(object sender, ActionControlEventArgs e) {
        if (e.ActionControl.ActionId is "AcceptLogonParameters" && e.ActionControl is DxToolbarItemActionControlBase toolbarItemAction) {
            toolbarItemAction.ToolbarItemModel.RenderStyle = ButtonRenderStyle.Primary;
        }
    }

    protected override void OnFrameAssigned() {
        base.OnFrameAssigned();
        Active[ControllerActiveKey] = !Application.Security.IsAuthenticated;
    }

    protected override void OnActivated() {
        base.OnActivated();
        actionControlsSiteController = Frame.GetController<ActionControlsSiteController>();
        if (actionControlsSiteController is not null) {
            actionControlsSiteController.CustomizeActionControl += ActionControlsSiteController_CustomizeActionControl;
        }
    }

    protected override void OnDeactivated() {
        base.OnDeactivated();
        if (actionControlsSiteController is not null) {
            actionControlsSiteController.CustomizeActionControl -= ActionControlsSiteController_CustomizeActionControl;
            actionControlsSiteController = null;
        }
    }
}
