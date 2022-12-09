using System;
using DevExpress.ExpressApp;
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
        }
    }
    public delegate IAuthenticationStandardUser CreateSecuritySystemUser(IObjectSpace objectSpace, string userName, string email, string password, bool isAdministrator);
}
