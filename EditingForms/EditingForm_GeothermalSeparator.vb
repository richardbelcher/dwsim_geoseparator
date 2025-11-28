Imports System.Windows.Forms
Imports System.Drawing
Imports DWSIM.Interfaces.Enums.GraphicObjects
Imports su = DWSIM.SharedClasses.SystemsOfUnits
Imports WeifenLuo.WinFormsUI.Docking
Imports DWSIM.UnitOperations.UnitOperations
Imports DWSIM.SharedClasses
Imports OxyPlot
Imports OxyPlot.Series
Imports OxyPlot.Axes
Imports OxyPlot.Annotations

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

                ' Update charts tab
                UpdateChartsTab()

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

                ' Velocity warnings (combined V_T and V_AN)
                Dim warnings As New List(Of String)
                If Not String.IsNullOrEmpty(.VelocityStatus) Then warnings.Add(.VelocityStatus)
                If Not String.IsNullOrEmpty(.AnnularVelocityStatus) Then warnings.Add(.AnnularVelocityStatus)
                If Not String.IsNullOrEmpty(.EquipmentTypeRecommendation) AndAlso Not .EquipmentTypeCorrect Then
                    warnings.Add(.EquipmentTypeRecommendation)
                End If

                lblVelocityWarning.Text = String.Join(Environment.NewLine, warnings)
                Dim allOK As Boolean = .VelocityInRange AndAlso .AnnularVelocityInRange AndAlso .EquipmentTypeCorrect
                lblVelocityWarning.ForeColor = If(allOK, Color.Green, Color.OrangeRed)

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

    Private Sub UpdateChartsTab()
        If SeparatorObject Is Nothing Then Exit Sub

        Try
            With SeparatorObject
                ' Update Baker coordinates label
                If .BakerBx > 0 OrElse .BakerBy > 0 Then
                    lblBakerCoords.Text = String.Format("Bx = {0:F1}, By = {1:F1}  ({2})",
                                                        .BakerBx, .BakerBy,
                                                        GetFlowPatternName(.DetectedFlowPattern))
                Else
                    lblBakerCoords.Text = "Bx = —, By = —"
                End If

                ' Create Baker chart model
                Dim model = CreateBakerChartModel(.BakerBx, .BakerBy)
                plotBakerChart.Model = model

            End With
        Catch
            lblBakerCoords.Text = "Bx = —, By = —"
            plotBakerChart.Model = Nothing
        End Try
    End Sub

    Private Function GetFlowPatternName(pattern As GeothermalSeparator.FlowPatterns) As String
        Select Case pattern
            Case GeothermalSeparator.FlowPatterns.Stratified
                Return "Stratified"
            Case GeothermalSeparator.FlowPatterns.Annular
                Return "Annular"
            Case GeothermalSeparator.FlowPatterns.Dispersed
                Return "Dispersed"
            Case GeothermalSeparator.FlowPatterns.PlugSlug
                Return "Plug/Slug"
            Case Else
                Return "Auto"
        End Select
    End Function

    Private Function CreateBakerChartModel(Bx As Double, By As Double) As PlotModel
        Dim model As New PlotModel()
        model.Title = "Baker Flow Pattern Map"
        model.TitleFontSize = 10

        ' Log-log axes
        Dim xAxis As New LogarithmicAxis()
        xAxis.Position = AxisPosition.Bottom
        xAxis.Title = "Bx = (G_L/G_G) × λ²"
        xAxis.Minimum = 1
        xAxis.Maximum = 100000
        xAxis.MajorGridlineStyle = LineStyle.Solid
        xAxis.MajorGridlineColor = OxyColor.FromRgb(220, 220, 220)
        model.Axes.Add(xAxis)

        Dim yAxis As New LogarithmicAxis()
        yAxis.Position = AxisPosition.Left
        yAxis.Title = "By = G_G / (λ × ψ)"
        yAxis.Minimum = 0.1
        yAxis.Maximum = 1000
        yAxis.MajorGridlineStyle = LineStyle.Solid
        yAxis.MajorGridlineColor = OxyColor.FromRgb(220, 220, 220)
        model.Axes.Add(yAxis)

        ' =====================================================
        ' BAKER FLOW PATTERN MAP BOUNDARIES
        ' Reference: Revistadechimie (Baker 1954 correlations)
        ' =====================================================

        ' 1. Stratified → Wavy boundary (bottom region)
        Dim stratWavyLine As New LineSeries()
        stratWavyLine.Title = "Strat→Wavy"
        stratWavyLine.Color = OxyColors.Green
        stratWavyLine.StrokeThickness = 1.5
        ' Y = -0.121X + 9.403 for 5 ≤ X ≤ 36.3
        For x As Double = 5 To 36.3 Step 1
            Dim y = -0.121 * x + 9.403
            If y > 0.1 Then stratWavyLine.Points.Add(New DataPoint(x, y))
        Next
        ' Y = -0.092X + 8.387 for 36.3 ≤ X ≤ 66.6
        For x As Double = 36.3 To 66.6 Step 1
            Dim y = -0.092 * x + 8.387
            If y > 0.1 Then stratWavyLine.Points.Add(New DataPoint(x, y))
        Next
        model.Series.Add(stratWavyLine)

        ' 2. Wavy/Stratified → Plug/Slug/Annular boundary
        Dim wavyPlugLine As New LineSeries()
        wavyPlugLine.Title = "Wavy→Slug"
        wavyPlugLine.Color = OxyColors.DarkGreen
        wavyPlugLine.StrokeThickness = 1.5
        ' Y = 1.52×10⁴ × X⁻²·⁰⁸² for 11 ≤ X ≤ 300
        For x As Double = 11 To 300 Step 5
            Dim y = 15200 * Math.Pow(x, -2.082)
            If y >= 0.1 AndAlso y <= 1000 Then wavyPlugLine.Points.Add(New DataPoint(x, y))
        Next
        model.Series.Add(wavyPlugLine)

        ' 3. Plug → Slug boundary
        Dim plugSlugLine As New LineSeries()
        plugSlugLine.Title = "Plug→Slug"
        plugSlugLine.Color = OxyColors.Purple
        plugSlugLine.StrokeThickness = 1.5
        ' Y = 3.512 × X⁻⁰·²⁴³ for 98.8 ≤ X ≤ 2213
        For x As Double = 98.8 To 2213 Step 20
            Dim y = 3.512 * Math.Pow(x, -0.243)
            If y >= 0.1 AndAlso y <= 1000 Then plugSlugLine.Points.Add(New DataPoint(x, y))
        Next
        model.Series.Add(plugSlugLine)

        ' 4. Slug → Annular boundary (multiple segments)
        Dim slugAnnularLine As New LineSeries()
        slugAnnularLine.Title = "Slug→Annular"
        slugAnnularLine.Color = OxyColors.Red
        slugAnnularLine.StrokeThickness = 1.5
        ' Y = 214.1 × X⁻⁰·⁸⁴⁸ for 30.3 ≤ X ≤ 55.5
        For x As Double = 30.3 To 55.5 Step 1
            Dim y = 214.1 * Math.Pow(x, -0.848)
            If y >= 0.1 AndAlso y <= 1000 Then slugAnnularLine.Points.Add(New DataPoint(x, y))
        Next
        ' Y = 21.55 × X⁻⁰·²⁷⁷ for 55.5 ≤ X ≤ 130.7
        For x As Double = 55.5 To 130.7 Step 2
            Dim y = 21.55 * Math.Pow(x, -0.277)
            If y >= 0.1 AndAlso y <= 1000 Then slugAnnularLine.Points.Add(New DataPoint(x, y))
        Next
        ' Y = 0.008X + 4.652 for 130.7 ≤ X ≤ 868.5
        For x As Double = 130.7 To 868.5 Step 20
            Dim y = 0.008 * x + 4.652
            If y >= 0.1 AndAlso y <= 1000 Then slugAnnularLine.Points.Add(New DataPoint(x, y))
        Next
        ' Y = 0.006X + 6.605 for 868.5 ≤ X ≤ 2975
        For x As Double = 868.5 To 2975 Step 50
            Dim y = 0.006 * x + 6.605
            If y >= 0.1 AndAlso y <= 1000 Then slugAnnularLine.Points.Add(New DataPoint(x, y))
        Next
        model.Series.Add(slugAnnularLine)

        ' 5. Annular → Dispersed boundary
        Dim annDispLine As New LineSeries()
        annDispLine.Title = "Ann→Dispersed"
        annDispLine.Color = OxyColors.Blue
        annDispLine.StrokeThickness = 1.5
        ' Y = 1.168×10⁴ × X⁻¹·⁰³² for 108.6 ≤ X ≤ 208
        For x As Double = 108.6 To 208 Step 5
            Dim y = 11680 * Math.Pow(x, -1.032)
            If y >= 0.1 AndAlso y <= 1000 Then annDispLine.Points.Add(New DataPoint(x, y))
        Next
        ' Y = 188.5 × X⁻⁰·²⁵⁵ for 208 ≤ X ≤ 634.4
        For x As Double = 208 To 634.4 Step 10
            Dim y = 188.5 * Math.Pow(x, -0.255)
            If y >= 0.1 AndAlso y <= 1000 Then annDispLine.Points.Add(New DataPoint(x, y))
        Next
        ' Y = 0.002X + 34.6 for 634.4 ≤ X ≤ 4620
        For x As Double = 634.4 To 4620 Step 50
            Dim y = 0.002 * x + 34.6
            If y >= 0.1 AndAlso y <= 1000 Then annDispLine.Points.Add(New DataPoint(x, y))
        Next
        model.Series.Add(annDispLine)

        ' 6. To Bubbly/Froth boundary
        Dim bubblyLine As New LineSeries()
        bubblyLine.Title = "→Bubbly"
        bubblyLine.Color = OxyColors.Orange
        bubblyLine.StrokeThickness = 1.5
        ' Y = 55.83×ln(X) - 427 for 2133 ≤ X ≤ 12000
        For x As Double = 2133 To 12000 Step 100
            Dim y = 55.83 * Math.Log(x) - 427
            If y >= 0.1 AndAlso y <= 1000 Then bubblyLine.Points.Add(New DataPoint(x, y))
        Next
        model.Series.Add(bubblyLine)

        ' Add region labels as annotations
        Dim lblDispersed As New Annotations.TextAnnotation()
        lblDispersed.Text = "DISPERSED"
        lblDispersed.TextPosition = New DataPoint(1000, 200)
        lblDispersed.FontSize = 9
        lblDispersed.TextColor = OxyColors.Blue
        lblDispersed.StrokeThickness = 0
        model.Annotations.Add(lblDispersed)

        Dim lblAnnular As New Annotations.TextAnnotation()
        lblAnnular.Text = "ANNULAR"
        lblAnnular.TextPosition = New DataPoint(10, 50)
        lblAnnular.FontSize = 9
        lblAnnular.TextColor = OxyColors.Red
        lblAnnular.StrokeThickness = 0
        model.Annotations.Add(lblAnnular)

        Dim lblSlug As New Annotations.TextAnnotation()
        lblSlug.Text = "SLUG"
        lblSlug.TextPosition = New DataPoint(200, 3)
        lblSlug.FontSize = 9
        lblSlug.TextColor = OxyColors.Purple
        lblSlug.StrokeThickness = 0
        model.Annotations.Add(lblSlug)

        Dim lblStratified As New Annotations.TextAnnotation()
        lblStratified.Text = "STRATIFIED"
        lblStratified.TextPosition = New DataPoint(20, 1)
        lblStratified.FontSize = 9
        lblStratified.TextColor = OxyColors.Green
        lblStratified.StrokeThickness = 0
        model.Annotations.Add(lblStratified)

        Dim lblBubbly As New Annotations.TextAnnotation()
        lblBubbly.Text = "BUBBLY"
        lblBubbly.TextPosition = New DataPoint(5000, 100)
        lblBubbly.FontSize = 9
        lblBubbly.TextColor = OxyColors.Orange
        lblBubbly.StrokeThickness = 0
        model.Annotations.Add(lblBubbly)

        ' Operating point
        If Bx > 0 AndAlso By > 0 Then
            Dim opPoint As New ScatterSeries()
            opPoint.Title = "Operating Point"
            opPoint.MarkerType = MarkerType.Circle
            opPoint.MarkerSize = 8
            opPoint.MarkerFill = OxyColors.Red
            opPoint.MarkerStroke = OxyColors.Black
            opPoint.MarkerStrokeThickness = 2
            opPoint.Points.Add(New ScatterPoint(Bx, By))
            model.Series.Add(opPoint)
        End If

        model.LegendPosition = LegendPosition.TopRight
        model.LegendFontSize = 7
        model.IsLegendVisible = False  ' Hide legend to reduce clutter

        Return model
    End Function

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
