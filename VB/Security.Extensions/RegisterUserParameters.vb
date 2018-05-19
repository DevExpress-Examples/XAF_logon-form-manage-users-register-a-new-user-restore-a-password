Imports System
Imports DevExpress.Xpo
Imports DevExpress.ExpressApp
Imports DevExpress.Data.Filtering
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Model
Imports DevExpress.Persistent.Validation
Imports DevExpress.Persistent.Base.Security
Imports DevExpress.ExpressApp.DC

Namespace Security.Extensions
	<DomainComponent, ModelDefault("Caption", "Register User"), ImageName("BO_User")>
	Public Class RegisterUserParameters
		Implements ILogonActionParameters

		Public Const ValidationContext As String = "RegisterUserContext"
		<RuleRequiredField(Nothing, ValidationContext)>
		Public Property UserName() As String
		<ModelDefault("IsPassword", "True")>
		Public Property Password() As String
		<RuleRequiredField(Nothing, ValidationContext), RuleRegularExpression(Nothing, ValidationContext, ManageUsersOnLogonController.EmailPattern)>
		Public Property Email() As String
		Public Sub Process(ByVal objectSpace As IObjectSpace) Implements ILogonActionParameters.Process
			Dim user As IAuthenticationStandardUser = TryCast(objectSpace.FindObject(SecurityExtensionsModule.SecuritySystemUserType, New BinaryOperator("UserName", UserName)), IAuthenticationStandardUser)
			If user IsNot Nothing Then
				Throw New ArgumentException("The login with the entered UserName or Email was already registered within the system")
			Else
				SecurityExtensionsModule.CreateSecuritySystemUser(objectSpace, UserName, Email, Password, False)
			End If
			'throw new UserFriendlyException("A new user has successfully been registered");
		End Sub
	End Class
End Namespace
