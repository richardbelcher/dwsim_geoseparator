Imports System.Windows.Forms
Imports DWSIM.Interfaces.Enums.GraphicObjects
Imports su = DWSIM.SharedClasses.SystemsOfUnits
Imports WeifenLuo.WinFormsUI.Docking
Imports DWSIM.UnitOperations.UnitOperations

Public Class EditingForm_GeothermalSeparator

    Inherits SharedClasses.ObjectEditorForm

    Public Property SeparatorObject As UnitOperations.GeothermalSeparator

    Public Loaded As Boolean = False

    Dim units As SharedClasses.SystemsOfUnits.Units
    Dim nf As String

    Private Sub EditingForm_GeothermalSeparator_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Float
        Me.Text = "Geothermal Separator"

        UpdateInfo()

    End Sub

    Public Sub UpdateInfo()

        If SeparatorObject Is Nothing Then Exit Sub

        units = SeparatorObject.FlowSheet.FlowsheetOptions.SelectedUnitSystem
        nf = SeparatorObject.FlowSheet.FlowsheetOptions.NumberFormat

        Loaded = False

        With SeparatorObject

            Me.Text = .GraphicObject.Tag & " (" & .GetDisplayName() & ")"

        End With

        Loaded = True

    End Sub

    Sub RequestCalc()

        SeparatorObject.FlowSheet.RequestCalculation(SeparatorObject)

    End Sub

End Class
