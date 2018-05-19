Imports System
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.DC
Imports DevExpress.ExpressApp.Security
Imports DevExpress.Persistent.Base.Security

Namespace Security.Extensions
	Public NotInheritable Partial Class SecurityExtensionsModule
		Inherits ModuleBase

		Public Shared SecuritySystemUserType As Type
		Public Shared CreateSecuritySystemUser As CreateSecuritySystemUser
		Public Sub New()
			InitializeComponent()
		End Sub
		Public Overrides Sub Setup(ByVal application As XafApplication)
			MyBase.Setup(application)
			AddHandler application.CreateCustomLogonWindowControllers, AddressOf application_CreateCustomLogonWindowControllers
		End Sub
		Public Overrides Sub Setup(ByVal moduleManager As ApplicationModulesManager)
			MyBase.Setup(moduleManager)
			If (Application IsNot Nothing) AndAlso (CreateSecuritySystemUser IsNot Nothing) Then
				Dim securityStrategy As SecurityStrategyComplex = TryCast(Application.Security, SecurityStrategyComplex)
				If securityStrategy IsNot Nothing Then
					SecuritySystemUserType = securityStrategy.UserType
				End If
			End If
		End Sub
		Private Sub application_CreateCustomLogonWindowControllers(ByVal sender As Object, ByVal e As CreateCustomLogonWindowControllersEventArgs)
			Dim app As XafApplication = DirectCast(sender, XafApplication)
			e.Controllers.Add(app.CreateController(Of ManageUsersOnLogonController)())
			' Dennis: Not needed starting with 15.1 (S35648).
			'e.Controllers.Add(app.CreateController<DevExpress.ExpressApp.Validation.ActionValidationController>());
			'e.Controllers.Add(app.CreateController<DevExpress.ExpressApp.Validation.ResultsHighlightController>());
		End Sub
		'Dennis: I want to avoid inheritance from SecuritySystemUser and will just add a new property dynamically.
		Public Overrides Sub CustomizeTypesInfo(ByVal typesInfo As ITypesInfo)
			MyBase.CustomizeTypesInfo(typesInfo)
			If Not DesignMode Then
				Dim emailMember As String = "Email"
				Dim ti As ITypeInfo = typesInfo.FindTypeInfo(SecuritySystemUserType)
				If ti IsNot Nothing Then
					Dim mi As IMemberInfo = ti.FindMember(emailMember)
					If mi Is Nothing Then
						mi = ti.CreateMember(emailMember, GetType(String))
					End If
				End If
			End If
		End Sub
	End Class
	Public Delegate Function CreateSecuritySystemUser(ByVal objectSpace As IObjectSpace, ByVal userName As String, ByVal email As String, ByVal password As String, ByVal isAdministrator As Boolean) As IAuthenticationStandardUser
End Namespace
