using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Win;

namespace ManageUsersOnLogon.Win {
    public partial class ManageUsersOnLogonWindowsFormsApplication {
        public ManageUsersOnLogonWindowsFormsApplication() {
            InitializeComponent();
            DelayedViewItemsInitialization = true;
            EnsureShowViewStrategy();//Q438300
        }
        protected override void OnLoggedOn(LogonEventArgs args) {//Q477869
            base.OnLoggedOn(args);
            ShowViewStrategy = CreateShowViewStrategy() as WinShowViewStrategyBase;
        }
    }
}
