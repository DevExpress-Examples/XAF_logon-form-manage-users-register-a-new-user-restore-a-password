using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System.ComponentModel;

namespace Security.Extensions;

[DomainComponent]
public abstract class LogonActionParametersBase {
    public const string ValidationContext = "CustomLogonActionsContext";
    public const string EmailPattern = @"^[_a-z0-9-]+(\.[_a-z0-9-]+)*@[a-z0-9-]+(\.[a-z0-9-]+)*(\.[a-z]{2,4})$";
}

[DomainComponent]
[ModelDefault("Caption", "Register User")]
[ImageName("BO_User")]
public class RegisterUserParameters : LogonActionParametersBase {
    [RuleRequiredField(null, ValidationContext)]
    public string UserName { get; set; }

    [RuleRequiredField(null, ValidationContext)]
    [RuleRegularExpression(null, ValidationContext, EmailPattern)]
    public string Email { get; set; }

    [ModelDefault("IsPassword", "True")]
    [RuleRequiredField(null, ValidationContext)]
    public string Password { get; set; }
}

[DomainComponent]
[ModelDefault("Caption", "Restore Password")]
[ImageName("Action_ResetPassword")]
public class RestorePasswordParameters : LogonActionParametersBase {
    [RuleRequiredField(null, ValidationContext)]
    [RuleRegularExpression(null, ValidationContext, EmailPattern)]
    public string Email { get; set; }
}

[DomainComponent]
[ModelDefault("Caption", "Set New Password")]
[ImageName("Action_ResetPassword")]
public class SetNewPasswordParameters : LogonActionParametersBase {
    [ModelDefault("IsPassword", "True")]
    [RuleRequiredField(null, ValidationContext)]
    public string Password { get; set; }

    [ModelDefault("IsPassword", "True")]
    [RuleRequiredField(null, ValidationContext)]
    [RuleValueComparison(null, ValidationContext, ValueComparisonType.Equals, nameof(Password),
        ParametersMode.Expression, CustomMessageTemplate = "Passwords are different.")]
    public string ConfirmPassword { get; set; }

    [Browsable(false)]
    public string Token { get; set; }
}