using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base.Security;

namespace Security.Extensions {
    public sealed partial class SecurityExtensionsModule : ModuleBase {
        public static Type SecuritySystemUserType;
        public static CreateSecuritySystemUser CreateSecuritySystemUser;
        public SecurityExtensionsModule() {
            InitializeComponent();
        }
        public override void Setup(XafApplication application) {
            base.Setup(application);
            application.CreateCustomLogonWindowControllers += application_CreateCustomLogonWindowControllers;
        }
        public override void Setup(ApplicationModulesManager moduleManager) {
            base.Setup(moduleManager);
            if((Application != null) && (CreateSecuritySystemUser != null)) {
                SecurityStrategyComplex securityStrategy = Application.Security as SecurityStrategyComplex;
                if(securityStrategy != null) {
                    SecuritySystemUserType = securityStrategy.UserType;
                }
            }
        }
        private void application_CreateCustomLogonWindowControllers(object sender, CreateCustomLogonWindowControllersEventArgs e) {
            XafApplication app = (XafApplication)sender;
            e.Controllers.Add(app.CreateController<ManageUsersOnLogonController>());
            // Dennis: Not needed starting with 15.1 (S35648).
            //e.Controllers.Add(app.CreateController<DevExpress.ExpressApp.Validation.ActionValidationController>());
            //e.Controllers.Add(app.CreateController<DevExpress.ExpressApp.Validation.ResultsHighlightController>());
        }
        //Dennis: I want to avoid inheritance from SecuritySystemUser and will just add a new property dynamically.
        public override void CustomizeTypesInfo(ITypesInfo typesInfo) {
            base.CustomizeTypesInfo(typesInfo);
            if(!DesignMode) {
                string emailMember = "Email";
                ITypeInfo ti = typesInfo.FindTypeInfo(SecuritySystemUserType);
                if(ti != null) {
                    IMemberInfo mi = ti.FindMember(emailMember);
                    if(mi == null) {
                        mi = ti.CreateMember(emailMember, typeof(string));
                    }
                }
            }
        }
    }
    public delegate IAuthenticationStandardUser CreateSecuritySystemUser(IObjectSpace objectSpace, string userName, string email, string password, bool isAdministrator);
}
