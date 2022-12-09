using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Model;
using System.Security.Cryptography;
using DevExpress.Persistent.Validation;
using DevExpress.Persistent.Base.Security;

namespace Security.Extensions {
    //Dennis: A base class for our logon parameters objects.
    [DomainComponent]
    public abstract class LogonActionParametersBase {
        public const string EmailPattern = @"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$";
        public const string ValidationContext = "RegisterUserContext";
        [RuleRequiredField(null, ValidationContext)]
        [RuleRegularExpression(null, ValidationContext, EmailPattern)]
        public string Email { get; set; }
        public abstract void ExecuteBusinessLogic(IObjectSpace objectSpace);
    }
    [DomainComponent]
    [ModelDefault("Caption", "Register User")]
    [ImageName("BO_User")]
    public class RegisterUserParameters : LogonActionParametersBase {
        [RuleRequiredField(null, ValidationContext)]
        public string UserName { get; set; }
        [ModelDefault("IsPassword", "True")]
        public string Password { get; set; }
        public override void ExecuteBusinessLogic(IObjectSpace objectSpace) {
            IAuthenticationStandardUser user = objectSpace.FindObject(SecurityExtensionsModule.SecuritySystemUserType, new BinaryOperator("UserName", UserName)) as IAuthenticationStandardUser;
            if (user != null) {
                throw new ArgumentException("The login with the entered UserName or Email was already registered within the system");
            }
            else {
                SecurityExtensionsModule.CreateSecuritySystemUser(objectSpace, UserName, Email, Password, false);
            }
        }
    }
    //Dennis: Take special note that this is just a demo and you may want to think of a more sophisticated solution for restoring passwords, e.g. like in https://www.devexpress.com/Support/Center/Question/Details/T449116
    [DomainComponent]
    [ModelDefault("Caption", "Restore Password")]
    [ImageName("Action_ResetPassword")]
    public class RestorePasswordParameters : LogonActionParametersBase {
        public override void ExecuteBusinessLogic(IObjectSpace objectSpace) {
            if (string.IsNullOrEmpty(Email)) {
                throw new ArgumentException("Email address is not specified!");
            }
            IAuthenticationStandardUser user = objectSpace.FindObject(SecurityExtensionsModule.SecuritySystemUserType, CriteriaOperator.Parse("Email = ?", Email)) as IAuthenticationStandardUser;
            if (user == null) {
                throw new ArgumentException("Cannot find registered users by the provided email address!");
            }
            byte[] randomBytes = new byte[6];
            new RNGCryptoServiceProvider().GetBytes(randomBytes);
            string password = Convert.ToBase64String(randomBytes);
            //Dennis: Resets the old password and generates a new one.
            user.SetPassword(password);
            user.ChangePasswordOnFirstLogon = true;
            objectSpace.CommitChanges();
            EmailLoginInformation(Email, password);
        }
        public static void EmailLoginInformation(string email, string password) {
            //Dennis:
            //if (success){
            //    Send an email with the login details here. Refer to http://msdn.microsoft.com/en-us/library/system.net.mail.mailmessage.aspx for more details.
            //}
            //else{
            //    throw new Exception("Failed!");  
            //}
        }
    }
}
