using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.EF;

namespace DXApplication1.Module.BusinessObjects {
    public class ApplicationUserLoginInfo : BaseObject, ISecurityUserLoginInfo {

        [Appearance("PasswordProvider", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
        public virtual string LoginProviderName { get; set; }

        [Appearance("PasswordProviderUserKey", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
        public virtual string ProviderUserKey { get; set; }

        public virtual ApplicationUser User { get; set; }

        object ISecurityUserLoginInfo.User => User;
    }
}
