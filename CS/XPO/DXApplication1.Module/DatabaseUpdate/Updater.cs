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
using Microsoft.Extensions.DependencyInjection;
using DevExpress.Persistent.Base.Security;
using DevExpress.Persistent.Validation;
using System.Security.AccessControl;

namespace DXApplication1.Module.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public static IAuthenticationStandardUser CreateUser(IObjectSpace objectSpace, string userName, string email, string password, bool isAdministrator) {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(email)) {
            throw new UserFriendlyException("User Name and Email address are not specified!");
        }
        var userManager = objectSpace.ServiceProvider.GetRequiredService<UserManager>();
        if (userManager.FindUserByName<ApplicationUser>(objectSpace, userName) != null) {
            throw new UserFriendlyException("A user already exists with this name.");
        }
        if (objectSpace.FirstOrDefault<ApplicationUser>(user => user.Email == email) != null) {
            throw new UserFriendlyException("A user already exists with this email.");
        }

        var role = isAdministrator ? GetAdminRole(objectSpace) : GetDefaultRole(objectSpace);
        var result = userManager.CreateUser<ApplicationUser>(objectSpace, userName, password, (user) => {
            user.Email = email;
            user.Roles.Add(role);
        });
        if (!result.Succeeded) {
            throw new UserFriendlyException("Error creating a new user.");
        }
        return result.User;
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();

        // The code below creates users and roles for testing purposes only.
        // In production code, you can create users and assign roles to them automatically, as described in the following help topic:
        // https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication
#if !RELEASE
        // If a role doesn't exist in the database, create this role
        var defaultRole = GetDefaultRole(ObjectSpace);
        var adminRole = GetAdminRole(ObjectSpace);

        ObjectSpace.CommitChanges(); //This line persists created object(s).

        string EmptyPassword = "";
        var userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();
        if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "User") == null) {
            CreateUser(ObjectSpace, "User", "user@example.com", EmptyPassword, isAdministrator: false);
        }
        if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Admin") == null) {
            CreateUser(ObjectSpace, "Admin", "admin@example.com", EmptyPassword, isAdministrator: true);
        }

        ObjectSpace.CommitChanges(); //This line persists created object(s).
#endif
    }
    public override void UpdateDatabaseBeforeUpdateSchema() {
        base.UpdateDatabaseBeforeUpdateSchema();
        //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
        //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
        //}
    }
    private static PermissionPolicyRole GetAdminRole(IObjectSpace objectSpace) {
        PermissionPolicyRole adminRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if(adminRole == null) {
            adminRole = objectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
            adminRole.IsAdministrative = true;
        }
        return adminRole;
    }
    private static PermissionPolicyRole GetDefaultRole(IObjectSpace objectSpace) {
        PermissionPolicyRole defaultRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
        if(defaultRole == null) {
            defaultRole = objectSpace.CreateObject<PermissionPolicyRole>();
            defaultRole.Name = "Default";

            defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
        }
        return defaultRole;
    }
}
