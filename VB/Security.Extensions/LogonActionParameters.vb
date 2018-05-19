Imports System
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.DC
Imports DevExpress.Data.Filtering
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Model
Imports System.Security.Cryptography
Imports DevExpress.Persistent.Validation
Imports DevExpress.Persistent.Base.Security

Namespace Security.Extensions
    'Dennis: A base class for our logon parameters objects.
    <DomainComponent> _
    Public MustInherit Class LogonActionParametersBase
        Public Const EmailPattern As String = "^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$"
        Public Const ValidationContext As String = "RegisterUserContext"
        <RuleRequiredField(Nothing, ValidationContext), RuleRegularExpression(Nothing, ValidationContext, EmailPattern)> _
        Public Property Email() As String
        Public MustOverride Sub ExecuteBusinessLogic(ByVal objectSpace As IObjectSpace)
    End Class
    <DomainComponent, ModelDefault("Caption", "Register User"), ImageName("BO_User")> _
    Public Class RegisterUserParameters
        Inherits LogonActionParametersBase

        <RuleRequiredField(Nothing, ValidationContext)> _
        Public Property UserName() As String
        <ModelDefault("IsPassword", "True")> _
        Public Property Password() As String
        Public Overrides Sub ExecuteBusinessLogic(ByVal objectSpace As IObjectSpace)
            Dim user As IAuthenticationStandardUser = TryCast(objectSpace.FindObject(SecurityExtensionsModule.SecuritySystemUserType, New BinaryOperator("UserName", UserName)), IAuthenticationStandardUser)
            If user IsNot Nothing Then
                Throw New ArgumentException("The login with the entered UserName or Email was already registered within the system")
            Else
                SecurityExtensionsModule.CreateSecuritySystemUser(objectSpace, UserName, Email, Password, False)
            End If
        End Sub
    End Class
    'Dennis: Take special note that this is just a demo and you may want to think of a more sophisticated solution for restoring passwords, e.g. like in https://www.devexpress.com/Support/Center/Question/Details/T449116
    <DomainComponent, ModelDefault("Caption", "Restore Password"), ImageName("Action_ResetPassword")> _
    Public Class RestorePasswordParameters
        Inherits LogonActionParametersBase

        Public Overrides Sub ExecuteBusinessLogic(ByVal objectSpace As IObjectSpace)
            If String.IsNullOrEmpty(Email) Then
                Throw New ArgumentException("Email address is not specified!")
            End If
            Dim user As IAuthenticationStandardUser = TryCast(objectSpace.FindObject(SecurityExtensionsModule.SecuritySystemUserType, CriteriaOperator.Parse("Email = ?", Email)), IAuthenticationStandardUser)
            If user Is Nothing Then
                Throw New ArgumentException("Cannot find registered users by the provided email address!")
            End If
            Dim randomBytes(5) As Byte
            CType(New RNGCryptoServiceProvider(), RNGCryptoServiceProvider).GetBytes(randomBytes)
            Dim password As String = Convert.ToBase64String(randomBytes)
            'Dennis: Resets the old password and generates a new one.
            user.SetPassword(password)
            user.ChangePasswordOnFirstLogon = True
            objectSpace.CommitChanges()
            EmailLoginInformation(Email, password)
        End Sub
        Public Shared Sub EmailLoginInformation(ByVal email As String, ByVal password As String)
            'Dennis:
            'if (success){
            '    Send an email with the login details here. Refer to http://msdn.microsoft.com/en-us/library/system.net.mail.mailmessage.aspx for more details.
            '}
            'else{
            '    throw new Exception("Failed!");  
            '}
        End Sub
    End Class
End Namespace
