Imports System
Imports DevExpress.ExpressApp
Imports DevExpress.Data.Filtering
Imports DevExpress.ExpressApp.Updating
Imports DevExpress.Persistent.Validation
Imports DevExpress.ExpressApp.Security.Strategy
Imports DevExpress.Persistent.Base.Security
Imports DevExpress.Persistent.BaseImpl.PermissionPolicy
Imports DevExpress.ExpressApp.Security
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.Persistent.Base

Namespace ManageUsersOnLogon.Module.DatabaseUpdate
    Public Class Updater
        Inherits ModuleUpdater

        Public Sub New(ByVal objectSpace As IObjectSpace, ByVal currentDBVersion As Version)
            MyBase.New(objectSpace, currentDBVersion)
        End Sub
        Public Overrides Sub UpdateDatabaseAfterUpdateSchema()
            MyBase.UpdateDatabaseAfterUpdateSchema()
            '            #Region "Create Users for the Complex Security Strategy"
            CreateUser(ObjectSpace, "Sam", "sam@example.com", String.Empty, True)
            CreateUser(ObjectSpace, "John", "john@example.com", String.Empty, False)
            ObjectSpace.CommitChanges()
            '            #End Region
        End Sub
        Public Shared Function CreateUser(ByVal os As IObjectSpace, ByVal userName As String, ByVal email As String, ByVal password As String, ByVal isAdministrator As Boolean) As IAuthenticationStandardUser
            If String.IsNullOrEmpty(userName) OrElse String.IsNullOrEmpty(email) Then
                Throw New ArgumentException("UserName and Email address are not specified!")
            End If
            Dim user As PermissionPolicyUser = os.FindObject(Of PermissionPolicyUser)(New BinaryOperator("UserName", userName))
            If user Is Nothing Then
                user = os.CreateObject(Of PermissionPolicyUser)()
                user.UserName = userName
                user.IsActive = True
                user.SetMemberValue("Email", email)
                user.SetPassword(password)
                Dim role As PermissionPolicyRole = If(isAdministrator, GetAdministratorRole(os), GetDefaultRole(os))
                user.Roles.Add(role)
                user.Save()
                If Validator.RuleSet.ValidateTarget(os, user, DefaultContexts.Save).State = ValidationState.Valid Then
                    os.CommitChanges()
                End If
            End If
            Return user
        End Function
        Public Shared Function GetAdministratorRole(ByVal os As IObjectSpace) As PermissionPolicyRole
            Dim administratorRole As PermissionPolicyRole = os.FindObject(Of PermissionPolicyRole)(New BinaryOperator("Name", "Administrator"))
            If administratorRole Is Nothing Then
                administratorRole = os.CreateObject(Of PermissionPolicyRole)()
                administratorRole.Name = "Administrator"
                administratorRole.IsAdministrative = True
            End If
            Return administratorRole
        End Function

        Private Shared Function GetDefaultRole(ByVal ObjectSpace As IObjectSpace) As PermissionPolicyRole
            Dim defaultRole As PermissionPolicyRole = ObjectSpace.FindObject(Of PermissionPolicyRole)(New BinaryOperator("Name", "Default"))
            If defaultRole Is Nothing Then
                defaultRole = ObjectSpace.CreateObject(Of PermissionPolicyRole)()
                defaultRole.Name = "Default"
                defaultRole.AddObjectPermission(Of PermissionPolicyUser)(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow)
                defaultRole.AddNavigationPermission("Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow)
                defaultRole.AddMemberPermission(Of PermissionPolicyUser)(SecurityOperations.Write, "ChangePasswordOnFirstLogon", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow)
                defaultRole.AddMemberPermission(Of PermissionPolicyUser)(SecurityOperations.Write, "StoredPassword", "[Oid] = CurrentUserId()", SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of PermissionPolicyRole)(SecurityOperations.Read, SecurityPermissionState.Deny)
                defaultRole.AddTypePermissionsRecursively(Of ModelDifference)(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of ModelDifferenceAspect)(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of ModelDifference)(SecurityOperations.Create, SecurityPermissionState.Allow)
                defaultRole.AddTypePermissionsRecursively(Of ModelDifferenceAspect)(SecurityOperations.Create, SecurityPermissionState.Allow)
            End If
            Return defaultRole
        End Function

    End Class
End Namespace