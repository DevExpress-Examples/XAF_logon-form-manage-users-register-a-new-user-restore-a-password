Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Win

Namespace ManageUsersOnLogon.Win
	Partial Public Class ManageUsersOnLogonWindowsFormsApplication
		Public Sub New()
			InitializeComponent()
			DelayedViewItemsInitialization = True
			EnsureShowViewStrategy() 'Q438300
		End Sub
		Protected Overrides Sub OnLoggedOn(ByVal args As LogonEventArgs) 'Q477869
			MyBase.OnLoggedOn(args)
			ShowViewStrategy = TryCast(CreateShowViewStrategy(), WinShowViewStrategyBase)
		End Sub
	End Class
End Namespace
