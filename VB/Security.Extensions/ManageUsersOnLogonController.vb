Imports System
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Utils
Imports DevExpress.ExpressApp.Actions
Imports DevExpress.ExpressApp.Editors
Imports DevExpress.ExpressApp.Security
Imports DevExpress.ExpressApp.Templates

Namespace Security.Extensions
    Public Class ManageUsersOnLogonController
        Inherits ViewController(Of DetailView)

        Protected Const LogonActionParametersActiveKey As String = "Active for ILogonActionParameters only"
        Private privateRegisterUserAction As SimpleAction
        Public Property RegisterUserAction() As SimpleAction
            Get
                Return privateRegisterUserAction
            End Get
            Private Set(ByVal value As SimpleAction)
                privateRegisterUserAction = value
            End Set
        End Property
        Private privateRestorePasswordAction As SimpleAction
        Public Property RestorePasswordAction() As SimpleAction
            Get
                Return privateRestorePasswordAction
            End Get
            Private Set(ByVal value As SimpleAction)
                privateRestorePasswordAction = value
            End Set
        End Property
        Private privateAcceptLogonParametersAction As SimpleAction
        Public Property AcceptLogonParametersAction() As SimpleAction
            Get
                Return privateAcceptLogonParametersAction
            End Get
            Private Set(ByVal value As SimpleAction)
                privateAcceptLogonParametersAction = value
            End Set
        End Property
        Private privateCancelLogonParametersAction As SimpleAction
        Public Property CancelLogonParametersAction() As SimpleAction
            Get
                Return privateCancelLogonParametersAction
            End Get
            Private Set(ByVal value As SimpleAction)
                privateCancelLogonParametersAction = value
            End Set
        End Property
        Public Sub New()
            'Dennis: Initialization of the Actions placed within the login window layout.
            RegisterUserAction = CreateLogonParametersAction("RegisterUser", "RegisterUserCategory", "Register User", "BO_User", "Register a new user within the system", GetType(RegisterUserParameters))
            RestorePasswordAction = CreateLogonParametersAction("RestorePassword", "RestorePasswordCategory", "Restore Password", "Action_ResetPassword", "Restore forgotten login information", GetType(RestorePasswordParameters))
            AcceptLogonParametersAction = New SimpleAction(Me, "AcceptLogonParameters", "PopupActions", Sub(s, e) AcceptParameters(TryCast(e.CurrentObject, LogonActionParametersBase)))
            AcceptLogonParametersAction.Caption = "OK"
            CancelLogonParametersAction = New SimpleAction(Me, "CancelLogonParameters", "PopupActions", Sub(s, e) CancelParameters(TryCast(e.CurrentObject, LogonActionParametersBase)))
            CancelLogonParametersAction.Caption = "Cancel"
        End Sub
        'Dennis: Ensures that our controller is active only when a user is not logged on.
        Protected Overrides Sub OnViewChanging(ByVal view As View)
            MyBase.OnViewChanging(view)
            Active(ControllerActiveKey) = Not SecuritySystem.IsAuthenticated
        End Sub
        'Dennis: Manages the activity of Actions within the logon window depending on the current context.
        Protected Overrides Sub OnViewControlsCreated()
            MyBase.OnViewControlsCreated()
            'Dennis: Manage the state of own Actions as well as dialog Actions of the LogonController class within the same logon Frame.
            Dim isLogonParametersActionView As Boolean = (View IsNot Nothing) AndAlso (View.ObjectTypeInfo IsNot Nothing) AndAlso View.ObjectTypeInfo.Implements(Of LogonActionParametersBase)()
            Dim lc As LogonController = Frame.GetController(Of LogonController)()
            If lc IsNot Nothing Then
                lc.AcceptAction.Active(LogonActionParametersActiveKey) = Not isLogonParametersActionView
                lc.CancelAction.Active(LogonActionParametersActiveKey) = Not isLogonParametersActionView
            End If
            AcceptLogonParametersAction.Active(LogonActionParametersActiveKey) = isLogonParametersActionView
            CancelLogonParametersAction.Active(LogonActionParametersActiveKey) = isLogonParametersActionView
        End Sub
        'Dennis: Creates a SimpleAction using the specified parameters.
        Private Function CreateLogonParametersAction(ByVal id As String, ByVal category As String, ByVal caption As String, ByVal imageName As String, ByVal toolTip As String, ByVal parametersType As Type) As SimpleAction
            Dim action As New SimpleAction(Me, id, category)
            action.Caption = caption
            action.ImageName = imageName
            action.PaintStyle = ActionItemPaintStyle.Image
            action.ToolTip = toolTip
            AddHandler action.Execute, Sub(s, e) CreateParametersViewCore(e)
            action.Tag = parametersType
            Return action
        End Function
        'Dennis: Configures a View used to display our parameters objects. 
        Protected Overridable Sub CreateParametersViewCore(ByVal e As SimpleActionExecuteEventArgs)
            Dim parametersType As Type = TryCast(e.Action.Tag, Type)
            Guard.ArgumentNotNull(parametersType, "parametersType")
            Dim logonActionParameters As Object = Activator.CreateInstance(parametersType)
            Dim dv As DetailView = Application.CreateDetailView(ObjectSpaceInMemory.CreateNew(), logonActionParameters)
            dv.ViewEditMode = ViewEditMode.Edit
            e.ShowViewParameters.CreatedView = dv
            e.ShowViewParameters.Context = TemplateContext.PopupWindow
            'WinForms:
            'e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;
            'ASP.NET:
            e.ShowViewParameters.TargetWindow = TargetWindow.Current
        End Sub
        Protected Overridable Sub AcceptParameters(ByVal parameters As LogonActionParametersBase)
            Guard.ArgumentNotNull(parameters, "parameters")
            parameters.ExecuteBusinessLogic(Application.CreateObjectSpace())
            'Dennis: Some additional UI-level logic is necessary depending on the parameters type.
            Dim registerUserParameters As RegisterUserParameters = TryCast(parameters, RegisterUserParameters)
            If registerUserParameters IsNot Nothing Then
                'Dennis: Perform automatic logon after a new user has been successfully registered.
                Dim lc As LogonController = Frame.GetController(Of LogonController)()
                If lc IsNot Nothing Then
                    lc.AcceptAction.Active.RemoveItem(LogonActionParametersActiveKey)
                    Dim securitySystemLogonParameters As AuthenticationStandardLogonParameters = DirectCast(SecuritySystem.LogonParameters, AuthenticationStandardLogonParameters)
                    securitySystemLogonParameters.UserName = registerUserParameters.UserName
                    securitySystemLogonParameters.Password = registerUserParameters.Password
                    lc.AcceptAction.DoExecute()
                End If
            Else
                CloseParametersView()
            End If
        End Sub
        Protected Overridable Sub CancelParameters(ByVal parameters As LogonActionParametersBase)
            CloseParametersView()
        End Sub
        Protected Overridable Sub CloseParametersView()
            View.Close(False)
            Application.LogOff()
        End Sub
    End Class
End Namespace