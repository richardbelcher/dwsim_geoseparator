Imports System.Windows.Forms
Imports System.Drawing
Imports DWSIM.Interfaces.Enums.GraphicObjects
Imports su = DWSIM.SharedClasses.SystemsOfUnits
Imports WeifenLuo.WinFormsUI.Docking
Imports DWSIM.UnitOperations.UnitOperations
Imports DWSIM.SharedClasses

Public Class EditingForm_GeothermalSeparator

    Inherits SharedClasses.ObjectEditorForm

    Public Property SeparatorObject As GeothermalSeparator

    Public Loaded As Boolean = False

    Private units As SharedClasses.SystemsOfUnits.Units
    Private nf As String

#Region "Form Events"

    Private Sub EditingForm_GeothermalSeparator_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.ShowHint = DockState.DockLeft
        UpdateInfo()
    End Sub

#End Region

#Region "UpdateInfo - Main UI Update Method"

    Public Sub UpdateInfo()
        If SeparatorObject Is Nothing Then Exit Sub
        If SeparatorObject.FlowSheet Is Nothing Then Exit Sub

        units = SeparatorObject.FlowSheet.FlowsheetOptions.SelectedUnitSystem
        nf = SeparatorObject.FlowSheet.FlowsheetOptions.NumberFormat

        Loaded = False

        Try
            With SeparatorObject
                ' Update form title
                Me.Text = .GraphicObject.Tag & " (" & .GetDisplayName() & ")"

                ' Update tag
                tbTag.Text = .GraphicObject.Tag

                ' Update active state
                chkActive.Checked = .GraphicObject.Active

                ' Update status
                UpdateStatus()

                ' Populate and select streams
                PopulateStreamComboBoxes()
                SelectConnectedStreams()

                ' Update calculation mode (enum index matches combo index)
                cbCalcMode.SelectedIndex = CInt(.CalculationMode)

                ' Update pressure calculation (enum: Average=0, Maximum=1, Minimum=2)
                cbPressureCalc.SelectedIndex = CInt(.PressureCalculation)

                ' Update override checkboxes and values
                chkOverrideT.Checked = .OverrideT
                chkOverrideP.Checked = .OverrideP

                ' Update temperature with unit conversion
                tbFlashTemperature.Text = su.Converter.ConvertFromSI(units.temperature, .FlashTemperature).ToString(nf)
                tbFlashTemperature.Enabled = .OverrideT

                ' Update pressure with unit conversion
                tbFlashPressure.Text = su.Converter.ConvertFromSI(units.pressure, .FlashPressure).ToString(nf)
                tbFlashPressure.Enabled = .OverrideP

                ' Update unit labels
                lblTempUnits.Text = units.temperature
                lblPressUnits.Text = units.pressure

                ' Update sizing parameters
                tbDimensionRatio.Text = .DimensionRatio.ToString(nf)
                tbSurgeFactor.Text = .SurgeFactor.ToString(nf)
                tbResidenceTime.Text = .ResidenceTime.ToString(nf)

                ' Update calculated sizing results
                If .DH > 0 Then
                    lblDiameterValue.Text = su.Converter.ConvertFromSI(units.diameter, .DH).ToString(nf) & " " & units.diameter
                Else
                    lblDiameterValue.Text = "—"
                End If

                If .AH > 0 Then
                    lblHeightValue.Text = su.Converter.ConvertFromSI(units.distance, .AH).ToString(nf) & " " & units.distance
                Else
                    lblHeightValue.Text = "—"
                End If

                ' Update results
                UpdateResults()

                ' Update efficiency tab
                UpdateEfficiencyTab()

            End With

        Catch ex As Exception
            ' Silently handle errors during update
        End Try

        Loaded = True
    End Sub

    Private Sub UpdateStatus()
        If SeparatorObject Is Nothing Then Exit Sub

        If SeparatorObject.GraphicObject.Calculated Then
            lblStatus.Text = "Calculated"
            lblStatus.ForeColor = Color.Blue
        ElseIf Not SeparatorObject.GraphicObject.Active Then
            lblStatus.Text = "Inactive"
            lblStatus.ForeColor = Color.Gray
        Else
            lblStatus.Text = "Not Calculated"
            lblStatus.ForeColor = Color.Black
        End If
    End Sub

    Private Sub UpdateResults()
        If SeparatorObject Is Nothing Then Exit Sub

        Try
            With SeparatorObject
                ' Heat duty
                If .DeltaQ.HasValue Then
                    lblDeltaQValue.Text = su.Converter.ConvertFromSI(units.heatflow, .DeltaQ.Value).ToString(nf) & " " & units.heatflow
                Else
                    lblDeltaQValue.Text = "—"
                End If

                ' Get vapor and liquid flows from MixedStream if available
                If .MixedStream IsNot Nothing Then
                    Dim vaporFlow = .MixedStream.Phases(2).Properties.massflow.GetValueOrDefault
                    Dim liquidFlow = .MixedStream.Phases(1).Properties.massflow.GetValueOrDefault
                    Dim vaporFraction = .MixedStream.Phases(2).Properties.molarfraction.GetValueOrDefault

                    lblVaporFlowValue.Text = su.Converter.ConvertFromSI(units.massflow, vaporFlow).ToString(nf) & " " & units.massflow
                    lblLiquidFlowValue.Text = su.Converter.ConvertFromSI(units.massflow, liquidFlow).ToString(nf) & " " & units.massflow
                    lblVaporFractionValue.Text = vaporFraction.ToString("F4")
                Else
                    lblVaporFlowValue.Text = "—"
                    lblLiquidFlowValue.Text = "—"
                    lblVaporFractionValue.Text = "—"
                End If
            End With
        Catch
            lblDeltaQValue.Text = "—"
            lblVaporFlowValue.Text = "—"
            lblLiquidFlowValue.Text = "—"
            lblVaporFractionValue.Text = "—"
        End Try
    End Sub

    Private Sub UpdateEfficiencyTab()
        If SeparatorObject Is Nothing Then Exit Sub

        Try
            With SeparatorObject
                ' Update inputs
                cbSizingMode.SelectedIndex = CInt(.SizingMode)
                cbFlowPattern.SelectedIndex = CInt(.FlowPattern)
                tbDesignVelocity.Text = .DesignSteamVelocity.ToString("F1")
                tbInletDiameter.Text = .InletPipeDiameter.ToString("F3")

                ' Update calculated results
                ' Equipment type
                lblEquipTypeValue.Text = If(.EquipmentType = GeothermalSeparator.SeparatorType.Separator, "Separator", "Dryer")

                ' Detected flow pattern
                Select Case .DetectedFlowPattern
                    Case GeothermalSeparator.FlowPatterns.Stratified
                        lblDetectedPatternValue.Text = "Stratified/Wavy"
                    Case GeothermalSeparator.FlowPatterns.Annular
                        lblDetectedPatternValue.Text = "Annular"
                    Case GeothermalSeparator.FlowPatterns.Dispersed
                        lblDetectedPatternValue.Text = "Dispersed/Bubble"
                    Case GeothermalSeparator.FlowPatterns.PlugSlug
                        lblDetectedPatternValue.Text = "Plug/Slug"
                    Case Else
                        lblDetectedPatternValue.Text = "Auto"
                End Select

                ' Dimensions
                If .VesselDiameter > 0 Then
                    lblVesselDiamValue.Text = su.Converter.ConvertFromSI(units.diameter, .VesselDiameter).ToString("F3") & " " & units.diameter
                Else
                    lblVesselDiamValue.Text = "—"
                End If

                If .TotalHeight > 0 Then
                    lblTotalHtValue.Text = su.Converter.ConvertFromSI(units.distance, .TotalHeight).ToString("F3") & " " & units.distance
                Else
                    lblTotalHtValue.Text = "—"
                End If

                ' Velocities
                If .ActualSteamVelocity > 0 Then
                    lblInletVelocityValue.Text = .ActualSteamVelocity.ToString("F1") & " m/s"
                Else
                    lblInletVelocityValue.Text = "—"
                End If

                If .AnnularVelocity > 0 Then
                    lblAnnularVelValue.Text = .AnnularVelocity.ToString("F2") & " m/s"
                Else
                    lblAnnularVelValue.Text = "—"
                End If

                ' Drop diameter
                If .DropDiameter > 0 Then
                    lblDropDiamValue.Text = .DropDiameter.ToString("F1") & " μm"
                Else
                    lblDropDiamValue.Text = "—"
                End If

                ' Efficiencies
                If .CentrifugalEfficiency > 0 Then
                    lblCentrifugalEffValue.Text = (.CentrifugalEfficiency * 100).ToString("F4") & " %"
                Else
                    lblCentrifugalEffValue.Text = "—"
                End If

                If .EntrainmentEfficiency > 0 Then
                    lblEntrainmentEffValue.Text = (.EntrainmentEfficiency * 100).ToString("F4") & " %"
                Else
                    lblEntrainmentEffValue.Text = "—"
                End If

                If .OverallEfficiency > 0 Then
                    lblOverallEffValue.Text = (.OverallEfficiency * 100).ToString("F4") & " %"
                    lblOverallEffValue.ForeColor = If(.OverallEfficiency >= 0.999, Color.Green, If(.OverallEfficiency >= 0.99, Color.Blue, Color.Red))
                Else
                    lblOverallEffValue.Text = "—"
                    lblOverallEffValue.ForeColor = Color.Black
                End If

                ' Outlet quality
                If .OutletSteamQuality > 0 Then
                    lblOutletQualityValue.Text = (.OutletSteamQuality * 100).ToString("F4") & " %"
                    lblOutletQualityValue.ForeColor = If(.OutletSteamQuality >= 0.9995, Color.Green, If(.OutletSteamQuality >= 0.999, Color.Blue, Color.Red))
                Else
                    lblOutletQualityValue.Text = "—"
                    lblOutletQualityValue.ForeColor = Color.Black
                End If

                ' Water carryover
                If .WaterCarryover >= 0 Then
                    lblCarryoverValue.Text = su.Converter.ConvertFromSI(units.massflow, .WaterCarryover).ToString("F4") & " " & units.massflow
                Else
                    lblCarryoverValue.Text = "—"
                End If

                ' Pressure drop
                If .SeparatorPressureDrop > 0 Then
                    lblSepPressureDropValue.Text = su.Converter.ConvertFromSI(units.deltaP, .SeparatorPressureDrop).ToString("F2") & " " & units.deltaP
                Else
                    lblSepPressureDropValue.Text = "—"
                End If

                ' Velocity warning
                lblVelocityWarning.Text = .VelocityStatus
                lblVelocityWarning.ForeColor = If(.VelocityInRange, Color.Green, Color.OrangeRed)

            End With
        Catch
            ' Reset all efficiency values on error
            lblEquipTypeValue.Text = "—"
            lblDetectedPatternValue.Text = "—"
            lblVesselDiamValue.Text = "—"
            lblTotalHtValue.Text = "—"
            lblInletVelocityValue.Text = "—"
            lblAnnularVelValue.Text = "—"
            lblDropDiamValue.Text = "—"
            lblCentrifugalEffValue.Text = "—"
            lblEntrainmentEffValue.Text = "—"
            lblOverallEffValue.Text = "—"
            lblOutletQualityValue.Text = "—"
            lblCarryoverValue.Text = "—"
            lblSepPressureDropValue.Text = "—"
            lblVelocityWarning.Text = ""
        End Try
    End Sub

#End Region

#Region "Stream Connection Management"

    Private Sub PopulateStreamComboBoxes()
        If SeparatorObject Is Nothing Then Exit Sub

        ' Store current selections
        Dim sel1 = If(cbInlet1.SelectedItem, "")
        Dim sel2 = If(cbInlet2.SelectedItem, "")
        Dim sel3 = If(cbInlet3.SelectedItem, "")
        Dim selV = If(cbVaporOut.SelectedItem, "")
        Dim selL = If(cbLiquidOut.SelectedItem, "")
        Dim selE = If(cbEnergy.SelectedItem, "")

        ' Clear all combo boxes
        cbInlet1.Items.Clear()
        cbInlet2.Items.Clear()
        cbInlet3.Items.Clear()
        cbVaporOut.Items.Clear()
        cbLiquidOut.Items.Clear()
        cbEnergy.Items.Clear()

        ' Add empty option
        cbInlet1.Items.Add("")
        cbInlet2.Items.Add("")
        cbInlet3.Items.Add("")
        cbVaporOut.Items.Add("")
        cbLiquidOut.Items.Add("")
        cbEnergy.Items.Add("")

        ' Populate from flowsheet objects
        For Each obj In SeparatorObject.FlowSheet.SimulationObjects.Values
            If TypeOf obj Is DWSIM.Thermodynamics.Streams.MaterialStream Then
                Dim tag = obj.GraphicObject.Tag
                cbInlet1.Items.Add(tag)
                cbInlet2.Items.Add(tag)
                cbInlet3.Items.Add(tag)
                cbVaporOut.Items.Add(tag)
                cbLiquidOut.Items.Add(tag)
            ElseIf obj.GraphicObject.ObjectType = ObjectType.EnergyStream Then
                cbEnergy.Items.Add(obj.GraphicObject.Tag)
            End If
        Next

        ' Restore selections if still valid
        If cbInlet1.Items.Contains(sel1) Then cbInlet1.SelectedItem = sel1
        If cbInlet2.Items.Contains(sel2) Then cbInlet2.SelectedItem = sel2
        If cbInlet3.Items.Contains(sel3) Then cbInlet3.SelectedItem = sel3
        If cbVaporOut.Items.Contains(selV) Then cbVaporOut.SelectedItem = selV
        If cbLiquidOut.Items.Contains(selL) Then cbLiquidOut.SelectedItem = selL
        If cbEnergy.Items.Contains(selE) Then cbEnergy.SelectedItem = selE
    End Sub

    Private Sub SelectConnectedStreams()
        If SeparatorObject Is Nothing Then Exit Sub

        Try
            ' Input connectors (0-5 are material, 6 is energy)
            For i As Integer = 0 To Math.Min(5, SeparatorObject.GraphicObject.InputConnectors.Count - 1)
                Dim conn = SeparatorObject.GraphicObject.InputConnectors(i)
                If conn.IsAttached AndAlso conn.AttachedConnector IsNot Nothing Then
                    Dim streamTag = conn.AttachedConnector.AttachedFrom.Tag
                    Select Case i
                        Case 0 : cbInlet1.SelectedItem = streamTag
                        Case 1 : cbInlet2.SelectedItem = streamTag
                        Case 2 : cbInlet3.SelectedItem = streamTag
                    End Select
                End If
            Next

            ' Energy connector (index 6)
            If SeparatorObject.GraphicObject.InputConnectors.Count > 6 Then
                Dim energyConn = SeparatorObject.GraphicObject.InputConnectors(6)
                If energyConn.IsAttached AndAlso energyConn.AttachedConnector IsNot Nothing Then
                    cbEnergy.SelectedItem = energyConn.AttachedConnector.AttachedFrom.Tag
                End If
            End If

            ' Output connectors
            If SeparatorObject.GraphicObject.OutputConnectors.Count > 0 Then
                Dim vaporConn = SeparatorObject.GraphicObject.OutputConnectors(0)
                If vaporConn.IsAttached AndAlso vaporConn.AttachedConnector IsNot Nothing Then
                    cbVaporOut.SelectedItem = vaporConn.AttachedConnector.AttachedTo.Tag
                End If
            End If

            If SeparatorObject.GraphicObject.OutputConnectors.Count > 1 Then
                Dim liquidConn = SeparatorObject.GraphicObject.OutputConnectors(1)
                If liquidConn.IsAttached AndAlso liquidConn.AttachedConnector IsNot Nothing Then
                    cbLiquidOut.SelectedItem = liquidConn.AttachedConnector.AttachedTo.Tag
                End If
            End If

        Catch ex As Exception
            ' Ignore connection errors
        End Try
    End Sub

#End Region

#Region "Connection Event Handlers"

    Private Sub cbInlet1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbInlet1.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        UpdateInletConnection(0, cbInlet1.SelectedItem?.ToString())
    End Sub

    Private Sub cbInlet2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbInlet2.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        UpdateInletConnection(1, cbInlet2.SelectedItem?.ToString())
    End Sub

    Private Sub cbInlet3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbInlet3.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        UpdateInletConnection(2, cbInlet3.SelectedItem?.ToString())
    End Sub

    Private Sub cbVaporOut_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbVaporOut.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        UpdateOutletConnection(0, cbVaporOut.SelectedItem?.ToString())
    End Sub

    Private Sub cbLiquidOut_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbLiquidOut.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        UpdateOutletConnection(1, cbLiquidOut.SelectedItem?.ToString())
    End Sub

    Private Sub cbEnergy_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbEnergy.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        UpdateInletConnection(6, cbEnergy.SelectedItem?.ToString())
    End Sub

    Private Sub UpdateInletConnection(index As Integer, streamTag As String)
        Try
            If index >= SeparatorObject.GraphicObject.InputConnectors.Count Then Exit Sub

            Dim connector = SeparatorObject.GraphicObject.InputConnectors(index)

            ' Disconnect existing connection
            If connector.IsAttached Then
                SeparatorObject.FlowSheet.DisconnectObjects(
                    connector.AttachedConnector.AttachedFrom,
                    SeparatorObject.GraphicObject)
            End If

            ' Make new connection if stream selected
            If Not String.IsNullOrEmpty(streamTag) Then
                Dim stream = SeparatorObject.FlowSheet.GetFlowsheetSimulationObject(streamTag)
                If stream IsNot Nothing Then
                    ' Find first available output connector on the stream
                    For j As Integer = 0 To stream.GraphicObject.OutputConnectors.Count - 1
                        If Not stream.GraphicObject.OutputConnectors(j).IsAttached Then
                            SeparatorObject.FlowSheet.ConnectObjects(
                                stream.GraphicObject,
                                SeparatorObject.GraphicObject,
                                j, index)
                            Exit For
                        End If
                    Next
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error connecting stream: " & ex.Message, "Connection Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub UpdateOutletConnection(index As Integer, streamTag As String)
        Try
            If index >= SeparatorObject.GraphicObject.OutputConnectors.Count Then Exit Sub

            Dim connector = SeparatorObject.GraphicObject.OutputConnectors(index)

            ' Disconnect existing connection
            If connector.IsAttached Then
                SeparatorObject.FlowSheet.DisconnectObjects(
                    SeparatorObject.GraphicObject,
                    connector.AttachedConnector.AttachedTo)
            End If

            ' Make new connection if stream selected
            If Not String.IsNullOrEmpty(streamTag) Then
                Dim stream = SeparatorObject.FlowSheet.GetFlowsheetSimulationObject(streamTag)
                If stream IsNot Nothing Then
                    ' Find first available input connector on the stream
                    For j As Integer = 0 To stream.GraphicObject.InputConnectors.Count - 1
                        If Not stream.GraphicObject.InputConnectors(j).IsAttached Then
                            SeparatorObject.FlowSheet.ConnectObjects(
                                SeparatorObject.GraphicObject,
                                stream.GraphicObject,
                                index, j)
                            Exit For
                        End If
                    Next
                End If
            End If
        Catch ex As Exception
            MessageBox.Show("Error connecting stream: " & ex.Message, "Connection Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    ' Disconnect button handlers
    Private Sub btnDisconnectInlet1_Click(sender As Object, e As EventArgs) Handles btnDisconnectInlet1.Click
        DisconnectInlet(0)
        cbInlet1.SelectedIndex = 0
    End Sub

    Private Sub btnDisconnectInlet2_Click(sender As Object, e As EventArgs) Handles btnDisconnectInlet2.Click
        DisconnectInlet(1)
        cbInlet2.SelectedIndex = 0
    End Sub

    Private Sub btnDisconnectInlet3_Click(sender As Object, e As EventArgs) Handles btnDisconnectInlet3.Click
        DisconnectInlet(2)
        cbInlet3.SelectedIndex = 0
    End Sub

    Private Sub btnDisconnectVaporOut_Click(sender As Object, e As EventArgs) Handles btnDisconnectVaporOut.Click
        DisconnectOutlet(0)
        cbVaporOut.SelectedIndex = 0
    End Sub

    Private Sub btnDisconnectLiquidOut_Click(sender As Object, e As EventArgs) Handles btnDisconnectLiquidOut.Click
        DisconnectOutlet(1)
        cbLiquidOut.SelectedIndex = 0
    End Sub

    Private Sub btnDisconnectEnergy_Click(sender As Object, e As EventArgs) Handles btnDisconnectEnergy.Click
        DisconnectInlet(6)
        cbEnergy.SelectedIndex = 0
    End Sub

    Private Sub DisconnectInlet(index As Integer)
        If SeparatorObject Is Nothing Then Exit Sub
        Try
            If index < SeparatorObject.GraphicObject.InputConnectors.Count Then
                Dim conn = SeparatorObject.GraphicObject.InputConnectors(index)
                If conn.IsAttached Then
                    SeparatorObject.FlowSheet.DisconnectObjects(
                        conn.AttachedConnector.AttachedFrom,
                        SeparatorObject.GraphicObject)
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub DisconnectOutlet(index As Integer)
        If SeparatorObject Is Nothing Then Exit Sub
        Try
            If index < SeparatorObject.GraphicObject.OutputConnectors.Count Then
                Dim conn = SeparatorObject.GraphicObject.OutputConnectors(index)
                If conn.IsAttached Then
                    SeparatorObject.FlowSheet.DisconnectObjects(
                        SeparatorObject.GraphicObject,
                        conn.AttachedConnector.AttachedTo)
                End If
            End If
        Catch ex As Exception
        End Try
    End Sub

    ' Create and connect button handlers
    Private Sub btnCreateInlet1_Click(sender As Object, e As EventArgs) Handles btnCreateInlet1.Click
        CreateAndConnectInletStream(0)
    End Sub

    Private Sub btnCreateInlet2_Click(sender As Object, e As EventArgs) Handles btnCreateInlet2.Click
        CreateAndConnectInletStream(1)
    End Sub

    Private Sub btnCreateInlet3_Click(sender As Object, e As EventArgs) Handles btnCreateInlet3.Click
        CreateAndConnectInletStream(2)
    End Sub

    Private Sub btnCreateVaporOut_Click(sender As Object, e As EventArgs) Handles btnCreateVaporOut.Click
        CreateAndConnectOutletStream(0, "Vapor")
    End Sub

    Private Sub btnCreateLiquidOut_Click(sender As Object, e As EventArgs) Handles btnCreateLiquidOut.Click
        CreateAndConnectOutletStream(1, "Liquid")
    End Sub

    Private Sub btnCreateEnergy_Click(sender As Object, e As EventArgs) Handles btnCreateEnergy.Click
        CreateAndConnectEnergyStream()
    End Sub

    Private Sub CreateAndConnectInletStream(index As Integer)
        If SeparatorObject Is Nothing Then Exit Sub
        Try
            Dim connPos = SeparatorObject.GraphicObject.InputConnectors(index).Position
            Dim newStream = SeparatorObject.FlowSheet.AddObject(
                ObjectType.MaterialStream,
                CInt(connPos.X) - 100,
                CInt(connPos.Y),
                SeparatorObject.GraphicObject.Tag & "_In" & (index + 1).ToString())

            ' Connect it
            SeparatorObject.FlowSheet.ConnectObjects(
                newStream.GraphicObject,
                SeparatorObject.GraphicObject,
                0, index)

            UpdateInfo()
        Catch ex As Exception
            MessageBox.Show("Error creating stream: " & ex.Message, "Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub CreateAndConnectOutletStream(index As Integer, suffix As String)
        If SeparatorObject Is Nothing Then Exit Sub
        Try
            Dim connPos = SeparatorObject.GraphicObject.OutputConnectors(index).Position
            Dim newStream = SeparatorObject.FlowSheet.AddObject(
                ObjectType.MaterialStream,
                CInt(connPos.X) + 50,
                CInt(connPos.Y),
                SeparatorObject.GraphicObject.Tag & "_" & suffix)

            ' Connect it
            SeparatorObject.FlowSheet.ConnectObjects(
                SeparatorObject.GraphicObject,
                newStream.GraphicObject,
                index, 0)

            UpdateInfo()
        Catch ex As Exception
            MessageBox.Show("Error creating stream: " & ex.Message, "Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub CreateAndConnectEnergyStream()
        If SeparatorObject Is Nothing Then Exit Sub
        Try
            Dim connPos = SeparatorObject.GraphicObject.InputConnectors(6).Position
            Dim newStream = SeparatorObject.FlowSheet.AddObject(
                ObjectType.EnergyStream,
                CInt(connPos.X) - 100,
                CInt(connPos.Y),
                SeparatorObject.GraphicObject.Tag & "_Energy")

            ' Connect it
            SeparatorObject.FlowSheet.ConnectObjects(
                newStream.GraphicObject,
                SeparatorObject.GraphicObject,
                0, 6)

            UpdateInfo()
        Catch ex As Exception
            MessageBox.Show("Error creating energy stream: " & ex.Message, "Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

#End Region

#Region "Property Event Handlers"

    ' Tag editing
    Private Sub tbTag_TextChanged(sender As Object, e As EventArgs) Handles tbTag.TextChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        SeparatorObject.GraphicObject.Tag = tbTag.Text
        Me.Text = tbTag.Text & " (" & SeparatorObject.GetDisplayName() & ")"
    End Sub

    Private Sub tbTag_KeyDown(sender As Object, e As KeyEventArgs) Handles tbTag.KeyDown
        If e.KeyCode = Keys.Enter Then
            RequestCalc()
        End If
    End Sub

    ' Active checkbox
    Private Sub chkActive_CheckedChanged(sender As Object, e As EventArgs) Handles chkActive.CheckedChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        SeparatorObject.GraphicObject.Active = chkActive.Checked
        UpdateStatus()
    End Sub

    ' Calculation mode
    Private Sub cbCalcMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbCalcMode.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        SeparatorObject.CalculationMode = CType(cbCalcMode.SelectedIndex, GeothermalSeparator.CalculationModes)
    End Sub

    ' Pressure calculation
    Private Sub cbPressureCalc_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbPressureCalc.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        SeparatorObject.PressureCalculation = CType(cbPressureCalc.SelectedIndex, GeothermalSeparator.PressureBehavior)
    End Sub

    ' Override temperature
    Private Sub chkOverrideT_CheckedChanged(sender As Object, e As EventArgs) Handles chkOverrideT.CheckedChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        SeparatorObject.OverrideT = chkOverrideT.Checked
        tbFlashTemperature.Enabled = chkOverrideT.Checked
    End Sub

    Private Sub tbFlashTemperature_TextChanged(sender As Object, e As EventArgs) Handles tbFlashTemperature.TextChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        Dim value As Double
        If Double.TryParse(tbFlashTemperature.Text, value) Then
            SeparatorObject.FlashTemperature = su.Converter.ConvertToSI(units.temperature, value)
            tbFlashTemperature.ForeColor = Color.Blue
        Else
            tbFlashTemperature.ForeColor = Color.Red
        End If
    End Sub

    Private Sub tbFlashTemperature_KeyDown(sender As Object, e As KeyEventArgs) Handles tbFlashTemperature.KeyDown
        If e.KeyCode = Keys.Enter Then
            RequestCalc()
        End If
    End Sub

    ' Override pressure
    Private Sub chkOverrideP_CheckedChanged(sender As Object, e As EventArgs) Handles chkOverrideP.CheckedChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        SeparatorObject.OverrideP = chkOverrideP.Checked
        tbFlashPressure.Enabled = chkOverrideP.Checked
    End Sub

    Private Sub tbFlashPressure_TextChanged(sender As Object, e As EventArgs) Handles tbFlashPressure.TextChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        Dim value As Double
        If Double.TryParse(tbFlashPressure.Text, value) Then
            SeparatorObject.FlashPressure = su.Converter.ConvertToSI(units.pressure, value)
            tbFlashPressure.ForeColor = Color.Blue
        Else
            tbFlashPressure.ForeColor = Color.Red
        End If
    End Sub

    Private Sub tbFlashPressure_KeyDown(sender As Object, e As KeyEventArgs) Handles tbFlashPressure.KeyDown
        If e.KeyCode = Keys.Enter Then
            RequestCalc()
        End If
    End Sub

    ' Sizing parameters
    Private Sub tbDimensionRatio_TextChanged(sender As Object, e As EventArgs) Handles tbDimensionRatio.TextChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        Dim value As Double
        If Double.TryParse(tbDimensionRatio.Text, value) AndAlso value > 0 Then
            SeparatorObject.DimensionRatio = value
            tbDimensionRatio.ForeColor = Color.Blue
        Else
            tbDimensionRatio.ForeColor = Color.Red
        End If
    End Sub

    Private Sub tbDimensionRatio_KeyDown(sender As Object, e As KeyEventArgs) Handles tbDimensionRatio.KeyDown
        If e.KeyCode = Keys.Enter Then
            RequestCalc()
        End If
    End Sub

    Private Sub tbSurgeFactor_TextChanged(sender As Object, e As EventArgs) Handles tbSurgeFactor.TextChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        Dim value As Double
        If Double.TryParse(tbSurgeFactor.Text, value) AndAlso value > 0 Then
            SeparatorObject.SurgeFactor = value
            tbSurgeFactor.ForeColor = Color.Blue
        Else
            tbSurgeFactor.ForeColor = Color.Red
        End If
    End Sub

    Private Sub tbSurgeFactor_KeyDown(sender As Object, e As KeyEventArgs) Handles tbSurgeFactor.KeyDown
        If e.KeyCode = Keys.Enter Then
            RequestCalc()
        End If
    End Sub

    Private Sub tbResidenceTime_TextChanged(sender As Object, e As EventArgs) Handles tbResidenceTime.TextChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        Dim value As Double
        If Double.TryParse(tbResidenceTime.Text, value) AndAlso value > 0 Then
            SeparatorObject.ResidenceTime = value
            tbResidenceTime.ForeColor = Color.Blue
        Else
            tbResidenceTime.ForeColor = Color.Red
        End If
    End Sub

    Private Sub tbResidenceTime_KeyDown(sender As Object, e As KeyEventArgs) Handles tbResidenceTime.KeyDown
        If e.KeyCode = Keys.Enter Then
            RequestCalc()
        End If
    End Sub

#End Region

#Region "Efficiency Tab Event Handlers"

    Private Sub cbSizingMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbSizingMode.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        SeparatorObject.SizingMode = CType(cbSizingMode.SelectedIndex, GeothermalSeparator.SizingModes)
        ' Enable/disable inlet diameter based on sizing mode
        tbInletDiameter.Enabled = (cbSizingMode.SelectedIndex = 1)  ' Rating mode
    End Sub

    Private Sub cbFlowPattern_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbFlowPattern.SelectedIndexChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        SeparatorObject.FlowPattern = CType(cbFlowPattern.SelectedIndex, GeothermalSeparator.FlowPatterns)
    End Sub

    Private Sub tbDesignVelocity_TextChanged(sender As Object, e As EventArgs) Handles tbDesignVelocity.TextChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        Dim value As Double
        If Double.TryParse(tbDesignVelocity.Text, value) AndAlso value > 0 Then
            SeparatorObject.DesignSteamVelocity = value
            tbDesignVelocity.ForeColor = Color.Blue
        Else
            tbDesignVelocity.ForeColor = Color.Red
        End If
    End Sub

    Private Sub tbDesignVelocity_KeyDown(sender As Object, e As KeyEventArgs) Handles tbDesignVelocity.KeyDown
        If e.KeyCode = Keys.Enter Then
            RequestCalc()
        End If
    End Sub

    Private Sub tbInletDiameter_TextChanged(sender As Object, e As EventArgs) Handles tbInletDiameter.TextChanged
        If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
        Dim value As Double
        If Double.TryParse(tbInletDiameter.Text, value) AndAlso value > 0 Then
            SeparatorObject.InletPipeDiameter = value
            tbInletDiameter.ForeColor = Color.Blue
        Else
            tbInletDiameter.ForeColor = Color.Red
        End If
    End Sub

    Private Sub tbInletDiameter_KeyDown(sender As Object, e As KeyEventArgs) Handles tbInletDiameter.KeyDown
        If e.KeyCode = Keys.Enter Then
            RequestCalc()
        End If
    End Sub

#End Region

#Region "Calculation Request"

    Private Sub RequestCalc()
        If SeparatorObject IsNot Nothing Then
            SeparatorObject.FlowSheet.RequestCalculation(SeparatorObject)
        End If
    End Sub

#End Region

End Class
