using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.SystemModule;

namespace Security.Extensions {
    public class ManageUsersOnLogonController : ViewController<DetailView> {
        protected const string LogonActionParametersActiveKey = "Active for ILogonActionParameters only";
        public const string EmailPattern = @"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$";
        public SimpleAction RegisterUserAction { get; private set; }
        public SimpleAction RestorePasswordAction { get; private set; }
        public SimpleAction AcceptLogonParametersAction { get; private set; }
        public SimpleAction CancelLogonParametersAction { get; private set; }


        public ManageUsersOnLogonController() {
            //Dennis: Initialization of the Actions placed within the login window layout.
            RegisterUserAction = CreateLogonSimpleAction("RegisterUser", "RegisterUserCategory", "Register User", "BO_User", "Register a new user within the system", typeof(RegisterUserParameters));
            RestorePasswordAction = CreateLogonSimpleAction("RestorePassword", "RestorePasswordCategory", "Restore Password", "Action_ResetPassword", "Restore forgotten login information", typeof(RestorePasswordParameters));
            AcceptLogonParametersAction = new SimpleAction(this, "AcceptLogonParameters", "PopupActions", (s, e) => {
                AcceptParameters(e.CurrentObject as ILogonActionParameters);
            }) { Caption = "OK" };
            CancelLogonParametersAction = new SimpleAction(this, "CancelLogonParameters", "PopupActions", (s, e) => {
                CancelParameters(e.CurrentObject as ILogonActionParameters);
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
            bool isLogonParametersActionView = GetLogonParametersActiveState();
            LogonController lc = Frame.GetController<LogonController>();
            if(lc != null) {
                lc.AcceptAction.Active[LogonActionParametersActiveKey] = !isLogonParametersActionView;
                lc.CancelAction.Active[LogonActionParametersActiveKey] = !isLogonParametersActionView;
            }
            AcceptLogonParametersAction.Active[LogonActionParametersActiveKey] = isLogonParametersActionView;
            CancelLogonParametersAction.Active[LogonActionParametersActiveKey] = isLogonParametersActionView;
        }
        //Dennis: Creates a SimpleAction using the specified parameters.
        private SimpleAction CreateLogonSimpleAction(string id, string category, string caption, string imageName, string toolTip, Type parametersType) {
            SimpleAction action = new SimpleAction(this, id, category);
            action.Caption = caption;
            action.ImageName = imageName;
            action.PaintStyle = ActionItemPaintStyle.Image;
            action.ToolTip = toolTip;
            action.Execute += CreateParametersViewDelegate;
            action.Tag = parametersType;
            return action;
        }
        //Dennis: Fires when our Actions are executed.
        private void CreateParametersViewDelegate(object sender, SimpleActionExecuteEventArgs e) {
            CreateParametersViewCore(e);
        }
        //Dennis: Configures a View used to display our parameters objects. 
        protected virtual void CreateParametersViewCore(SimpleActionExecuteEventArgs e) {
            Type parametersType = e.Action.Tag as Type;
            Guard.ArgumentNotNull(parametersType, "parametersType");
            DetailView dv = Application.CreateDetailView(ObjectSpaceInMemory.CreateNew(), Activator.CreateInstance(parametersType));
            dv.ViewEditMode = ViewEditMode.Edit;
            e.ShowViewParameters.CreatedView = dv;
            e.ShowViewParameters.Context = TemplateContext.PopupWindow;
            //WinForms:
            //e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;
            //ASP.NET:
            e.ShowViewParameters.TargetWindow = TargetWindow.Current;
        }
        protected virtual void AcceptParameters(ILogonActionParameters parameters) {
            if(parameters != null) {
                parameters.Process(Application.CreateObjectSpace());
                CloseParametersView();
            }
        }
        protected virtual void CancelParameters(ILogonActionParameters parameters) {
            CloseParametersView();
        }
        protected virtual void CloseParametersView() {
            View.Close(false);
            Application.LogOff();
        }
        //Dennis: Determines whether we are in the context of the LogonActionParametersBase object.
        protected virtual bool GetLogonParametersActiveState() {
            return View != null && View.ObjectTypeInfo != null && View.ObjectTypeInfo.Implements<ILogonActionParameters>();
        }
    }
    //Dennis: A base class for our logon parameters objects.
    public interface ILogonActionParameters {
        void Process(IObjectSpace objectSpace);
    }
}