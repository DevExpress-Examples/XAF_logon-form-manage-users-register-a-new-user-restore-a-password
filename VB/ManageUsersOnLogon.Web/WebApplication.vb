Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Security
Imports DevExpress.ExpressApp.Security.ClientServer
Imports DevExpress.ExpressApp.Web
Imports DevExpress.ExpressApp.Xpo
Imports DevExpress.Persistent.BaseImpl.PermissionPolicy

Namespace ManageUsersOnLogon.Web
    Partial Public Class ManageUsersOnLogonAspNetApplication
        Inherits WebApplication

        Private module1 As DevExpress.ExpressApp.SystemModule.SystemModule
        Private module2 As DevExpress.ExpressApp.Web.SystemModule.SystemAspNetModule
        Private module3 As ManageUsersOnLogon.Module.ManageUsersOnLogonModule
        'private ManageUsersOnLogon.Module.Web.ManageUsersOnLogonAspNetModule module4;
        Private securityModule1 As DevExpress.ExpressApp.Security.SecurityModule
        Private validationModule1 As DevExpress.ExpressApp.Validation.ValidationModule
        Private authenticationStandard1 As DevExpress.ExpressApp.Security.AuthenticationStandard
        Private securityStrategyComplex1 As DevExpress.ExpressApp.Security.SecurityStrategyComplex
        Private securityExtensionsModule1 As Security.Extensions.SecurityExtensionsModule

        Public Sub New()
            InitializeComponent()
            Dim secur = (CType(Security, SecurityStrategy))
            secur.AnonymousAllowedTypes.Add(GetType(PermissionPolicyUser))
            secur.AnonymousAllowedTypes.Add(GetType(PermissionPolicyRole))
        End Sub

        Protected Overrides Sub CreateDefaultObjectSpaceProvider(ByVal args As CreateCustomObjectSpaceProviderEventArgs)
            args.ObjectSpaceProvider = New SecuredObjectSpaceProvider(DirectCast(Me.Security, ISelectDataSecurityProvider), GetDataStoreProvider(args.ConnectionString, args.Connection), True)
        End Sub
        Private Function GetDataStoreProvider(ByVal connectionString As String, ByVal connection As System.Data.IDbConnection) As IXpoDataStoreProvider
            Dim application As System.Web.HttpApplicationState = If((System.Web.HttpContext.Current IsNot Nothing), System.Web.HttpContext.Current.Application, Nothing)
            Dim dataStoreProvider As IXpoDataStoreProvider = Nothing
            If Not application Is Nothing AndAlso application("DataStoreProvider") IsNot Nothing Then
                dataStoreProvider = TryCast(application("DataStoreProvider"), IXpoDataStoreProvider)
            Else
                dataStoreProvider = XPObjectSpaceProvider.GetDataStoreProvider(connectionString, connection, True)
                If Not application Is Nothing Then
                    application("DataStoreProvider") = dataStoreProvider
                End If
            End If
            Return dataStoreProvider
        End Function
        Private Sub ManageUsersOnLogonAspNetApplication_DatabaseVersionMismatch(ByVal sender As Object, ByVal e As DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs) Handles MyBase.DatabaseVersionMismatch
#If EASYTEST Then
            e.Updater.Update()
            e.Handled = True
#Else
            If System.Diagnostics.Debugger.IsAttached Then
                e.Updater.Update()
                e.Handled = True
            Else
                Dim message As String = "The application cannot connect to the specified database, because the latter doesn't exist or its version is older than that of the application." & ControlChars.CrLf & "This error occurred  because the automatic database update was disabled when the application was started without debugging." & ControlChars.CrLf & "To avoid this error, you should either start the application under Visual Studio in debug mode, or modify the " & "source code of the 'DatabaseVersionMismatch' event handler to enable automatic database update, " & "or manually create a database using the 'DBUpdater' tool." & ControlChars.CrLf & "Anyway, refer to the following help topics for more detailed information:" & ControlChars.CrLf & "'Update Application and Database Versions' at http://www.devexpress.com/Help/?document=ExpressApp/CustomDocument2795.htm" & ControlChars.CrLf & "'Database Security References' at http://www.devexpress.com/Help/?document=ExpressApp/CustomDocument3237.htm" & ControlChars.CrLf & "If this doesn't help, please contact our Support Team at http://www.devexpress.com/Support/Center/"

                If e.CompatibilityError IsNot Nothing AndAlso e.CompatibilityError.Exception IsNot Nothing Then
                    message &= ControlChars.CrLf & ControlChars.CrLf & "Inner exception: " & e.CompatibilityError.Exception.Message
                End If
                Throw New InvalidOperationException(message)
            End If
#End If
        End Sub

        Private Sub InitializeComponent()
            Me.module1 = New DevExpress.ExpressApp.SystemModule.SystemModule()
            Me.module2 = New DevExpress.ExpressApp.Web.SystemModule.SystemAspNetModule()
            Me.securityModule1 = New DevExpress.ExpressApp.Security.SecurityModule()
            Me.validationModule1 = New DevExpress.ExpressApp.Validation.ValidationModule()
            Me.authenticationStandard1 = New DevExpress.ExpressApp.Security.AuthenticationStandard()
            Me.securityStrategyComplex1 = New DevExpress.ExpressApp.Security.SecurityStrategyComplex()
            Me.module3 = New ManageUsersOnLogon.Module.ManageUsersOnLogonModule()
            Me.securityExtensionsModule1 = New Security.Extensions.SecurityExtensionsModule()
            DirectCast(Me, System.ComponentModel.ISupportInitialize).BeginInit()
            ' 
            ' authenticationStandard1
            ' 
            Me.authenticationStandard1.LogonParametersType = GetType(DevExpress.ExpressApp.Security.AuthenticationStandardLogonParameters)
            ' 
            ' securityStrategyComplex1
            ' 
            Me.securityStrategyComplex1.Authentication = Me.authenticationStandard1
            Me.securityStrategyComplex1.RoleType = GetType(DevExpress.ExpressApp.Security.Strategy.SecuritySystemRole)
            Me.securityStrategyComplex1.UserType = GetType(DevExpress.ExpressApp.Security.Strategy.SecuritySystemUser)
            ' 
            ' ManageUsersOnLogonAspNetApplication
            ' 
            Me.ApplicationName = "ManageUsersOnLogon"
            Me.Modules.Add(Me.module1)
            Me.Modules.Add(Me.module2)
            Me.Modules.Add(Me.securityModule1)
            Me.Modules.Add(Me.validationModule1)
            Me.Modules.Add(Me.securityExtensionsModule1)
            Me.Modules.Add(Me.module3)
            Me.Security = Me.securityStrategyComplex1
            DirectCast(Me, System.ComponentModel.ISupportInitialize).EndInit()

        End Sub
    End Class
End Namespace
