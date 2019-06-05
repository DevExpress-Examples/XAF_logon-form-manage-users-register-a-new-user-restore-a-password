using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Templates;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Validation;

namespace Security.Extensions {
    public class ManageUsersOnLogonController : ViewController<DetailView> {
        protected const string LogonActionParametersActiveKey = "Active for ILogonActionParameters only";
        public SimpleAction RegisterUserAction { get; private set; }
        public SimpleAction RestorePasswordAction { get; private set; }
        public SimpleAction AcceptLogonParametersAction { get; private set; }
        public SimpleAction CancelLogonParametersAction { get; private set; }
        public ManageUsersOnLogonController() {
            //Dennis: Initialization of the Actions placed within the login window layout.
            string defaltCategory = PredefinedCategory.PopupActions.ToString();
            RegisterUserAction = CreateLogonParametersAction("RegisterUser", defaltCategory, "Register User", "BO_User", "Register a new user within the system", typeof(RegisterUserParameters));
            RestorePasswordAction = CreateLogonParametersAction("RestorePassword", defaltCategory, "Restore Password", "Action_ResetPassword", "Restore forgotten login information", typeof(RestorePasswordParameters));
            AcceptLogonParametersAction = new SimpleAction(this, "AcceptLogonParameters", defaltCategory, (s, e) => {
                AcceptParameters(e.CurrentObject as LogonActionParametersBase);
            }) { Caption = "OK" };
            CancelLogonParametersAction = new SimpleAction(this, "CancelLogonParameters", defaltCategory, (s, e) => {
                CancelParameters(e.CurrentObject as LogonActionParametersBase);
            }) { Caption = "Cancel" };
        }
        //Dennis: Ensures that our controller is active only when a user is not logged on.
        protected override void OnViewChanging(View view) {
            base.OnViewChanging(view);
            Active[ControllerActiveKey] = !SecuritySystem.IsAuthenticated;
        }
        //Dennis: Manages the activity of Actions within the logon window depending on the current context.
        protected override void OnViewControlsCreated() {
            base.OnViewControlsCreated();
            //Dennis: Manage the state of own Actions as well as dialog Actions of the LogonController class within the same logon Frame.
            bool isLogonParametersActionView = (View != null) && (View.ObjectTypeInfo != null) && View.ObjectTypeInfo.Implements<LogonActionParametersBase>();
            LogonController lc = Frame.GetController<LogonController>();
            if (lc != null) {
                lc.AcceptAction.Active[LogonActionParametersActiveKey] = !isLogonParametersActionView;
                lc.CancelAction.Active[LogonActionParametersActiveKey] = !isLogonParametersActionView;
            }
            AcceptLogonParametersAction.Active[LogonActionParametersActiveKey] = isLogonParametersActionView;
            CancelLogonParametersAction.Active[LogonActionParametersActiveKey] = isLogonParametersActionView;
            RegisterUserAction.Active[LogonActionParametersActiveKey] = !isLogonParametersActionView;
            RestorePasswordAction.Active[LogonActionParametersActiveKey] = !isLogonParametersActionView;
        }
        //Dennis: Creates a SimpleAction using the specified parameters.
        private SimpleAction CreateLogonParametersAction(string id, string category, string caption, string imageName, string toolTip, Type parametersType) {
            SimpleAction action = new SimpleAction(this, id, category);
            action.Caption = caption;
            action.ImageName = imageName;
            action.PaintStyle = ActionItemPaintStyle.Image;
            action.ToolTip = toolTip;
            action.Execute += (s, e) => CreateParametersViewCore(e);
            action.Tag = parametersType;
            return action;
        }
        //Dennis: Configures a View used to display our parameters objects. 
        protected virtual void CreateParametersViewCore(SimpleActionExecuteEventArgs e) {
            Application.Modules.FindModule<ValidationModule>().InitializeRuleSet();//T461096, T390268
            Type parametersType = e.Action.Tag as Type;
            Guard.ArgumentNotNull(parametersType, "parametersType");
            object logonActionParameters = Activator.CreateInstance(parametersType);
            DetailView dv = Application.CreateDetailView(ObjectSpaceInMemory.CreateNew(), logonActionParameters);
            dv.ViewEditMode = ViewEditMode.Edit;
            e.ShowViewParameters.CreatedView = dv;
            e.ShowViewParameters.Context = TemplateContext.View;
            //WinForms:
            //e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;
            //ASP.NET:
            e.ShowViewParameters.TargetWindow = TargetWindow.Current;
        }
        protected virtual void AcceptParameters(LogonActionParametersBase parameters) {
            Guard.ArgumentNotNull(parameters, "parameters");
            parameters.ExecuteBusinessLogic(Application.CreateObjectSpace(SecurityExtensionsModule.SecuritySystemUserType));
            //Dennis: Some additional UI-level logic is necessary depending on the parameters type.
            RegisterUserParameters registerUserParameters = parameters as RegisterUserParameters;
            if (registerUserParameters != null) {
                //Dennis: Perform automatic logon after a new user has been successfully registered.
                LogonController lc = Frame.GetController<LogonController>();
                if (lc != null) {
                    lc.AcceptAction.Active.RemoveItem(LogonActionParametersActiveKey);
                    AuthenticationStandardLogonParameters securitySystemLogonParameters = (AuthenticationStandardLogonParameters)SecuritySystem.LogonParameters;
                    securitySystemLogonParameters.UserName = registerUserParameters.UserName;
                    securitySystemLogonParameters.Password = registerUserParameters.Password;
                    lc.AcceptAction.DoExecute();
                }
            }
            else {
                CloseParametersView();
            }
        }
        protected virtual void CancelParameters(LogonActionParametersBase parameters) {
            CloseParametersView();
        }
        protected virtual void CloseParametersView() {
            View.Close(false);
            Application.LogOff();
        }
    }
}