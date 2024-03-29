﻿using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DXApplication1.Module.BusinessObjects;
using DevExpress.Persistent.Base.Security;
using DevExpress.Persistent.Validation;

namespace DXApplication1.Module.DatabaseUpdate {
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion) {
        }
        public static IAuthenticationStandardUser CreateUser(IObjectSpace objectSpace, string userName, string email, string password, bool isAdministrator) {
            if(string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(email)) {
                throw new ArgumentException("UserName and Email address are not specified!");
            }
            ApplicationUser user = objectSpace.FindObject<ApplicationUser>(new BinaryOperator("UserName", userName));
            if(user == null) {
                user = objectSpace.CreateObject<ApplicationUser>();
                user.UserName = userName;
                user.IsActive = true;
                user.SetMemberValue("Email", email);
                user.SetPassword(password);
                PermissionPolicyRole role = isAdministrator ? GetAdministratorRole(objectSpace) : GetDefaultRole(objectSpace);
                user.Roles.Add(role);
                user.Save();
                if(Validator.RuleSet.ValidateTarget(objectSpace, user, DefaultContexts.Save).State == ValidationState.Valid) {
                    // The UserLoginInfo object requires a user object Id (Oid).
                    // Commit the user object to the database before you create a UserLoginInfo object. This will correctly initialize the user key property.
                    objectSpace.CommitChanges();
                    ((ISecurityUserWithLoginInfo)user).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, objectSpace.GetKeyValueAsString(user));
                    objectSpace.CommitChanges();
                }
            }
            return user;
        }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();
            ApplicationUser sampleUser = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == "User");
            if(sampleUser == null) {
                CreateUser(ObjectSpace, "User", "test@mail.com", "", false);
            }
            ApplicationUser userAdmin = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == "Admin");
            if(userAdmin == null) {
                CreateUser(ObjectSpace, "Admin", "admin@mail.com", "", true);
            }
        }
        static PermissionPolicyRole GetAdministratorRole(IObjectSpace os) {
            PermissionPolicyRole adminRole = os.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
            if(adminRole == null) {
                adminRole = os.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
            }
            adminRole.IsAdministrative = true;
            return adminRole;
        }
        public override void UpdateDatabaseBeforeUpdateSchema() {
            base.UpdateDatabaseBeforeUpdateSchema();
            //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
            //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
            //}
        }
        static PermissionPolicyRole GetDefaultRole(IObjectSpace os) {
            PermissionPolicyRole defaultRole = os.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
            if(defaultRole == null) {
                defaultRole = os.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default";

                defaultRole.AddObjectPermissionFromLambda<PermissionPolicyUser>(SecurityOperations.Read, cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<PermissionPolicyUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<PermissionPolicyUser>(SecurityOperations.Write, "StoredPassword", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
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
