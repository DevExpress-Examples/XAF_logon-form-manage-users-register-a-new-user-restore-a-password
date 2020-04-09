using System;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Persistent.Base.Security;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Base;

namespace ManageUsersOnLogon.Module.DatabaseUpdate {
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) : base(objectSpace, currentDBVersion) { }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();
            #region Create Users for the Complex Security Strategy
            CreateUser(ObjectSpace, "Sam", "sam@example.com", string.Empty, true);
            CreateUser(ObjectSpace, "John", "john@example.com", string.Empty, false);
            ObjectSpace.CommitChanges();
            #endregion
        }
        public static IAuthenticationStandardUser CreateUser(IObjectSpace os, string userName, string email, string password, bool isAdministrator) {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(email)) {
                throw new ArgumentException("UserName and Email address are not specified!");
            }
            PermissionPolicyUser user = os.FindObject<PermissionPolicyUser>(new BinaryOperator("UserName", userName));
            if (user == null) {
                user = os.CreateObject<PermissionPolicyUser>();
                user.UserName = userName;
                user.IsActive = true;
                user.SetMemberValue("Email", email);
                user.SetPassword(password);
                PermissionPolicyRole role = isAdministrator ? GetAdministratorRole(os) : GetDefaultRole(os);
                user.Roles.Add(role);
                user.Save();
                if (Validator.RuleSet.ValidateTarget(os, user, DefaultContexts.Save).State == ValidationState.Valid) {
                    os.CommitChanges();
                }
            }
            return user;
        }
        public static PermissionPolicyRole GetAdministratorRole(IObjectSpace os) {
            PermissionPolicyRole administratorRole = os.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "Administrator"));
            if (administratorRole == null) {
                administratorRole = os.CreateObject<PermissionPolicyRole>();
                administratorRole.Name = "Administrator";
                administratorRole.IsAdministrative = true;
            }
            return administratorRole;
        }
        private static PermissionPolicyRole GetDefaultRole(IObjectSpace ObjectSpace) {
            PermissionPolicyRole defaultRole = ObjectSpace.FindObject<PermissionPolicyRole>(new BinaryOperator("Name", "Default"));
            if(defaultRole == null) {
                defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default";

                defaultRole.AddObjectPermission<PermissionPolicyUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                defaultRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                defaultRole.AddMemberPermission<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
                defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
            }
            return defaultRole;
        }
    }
}