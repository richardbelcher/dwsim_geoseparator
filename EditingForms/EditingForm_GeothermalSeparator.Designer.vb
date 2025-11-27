<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class EditingForm_GeothermalSeparator
    Inherits SharedClasses.ObjectEditorForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()

        ' GroupBox for Status
        Me.GroupBoxStatus = New System.Windows.Forms.GroupBox()
        Me.lblTagLabel = New System.Windows.Forms.Label()
        Me.tbTag = New System.Windows.Forms.TextBox()
        Me.chkActive = New System.Windows.Forms.CheckBox()
        Me.lblStatusLabel = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()

        ' GroupBox for Connections
        Me.GroupBoxConnections = New System.Windows.Forms.GroupBox()
        Me.lblInlet1 = New System.Windows.Forms.Label()
        Me.cbInlet1 = New System.Windows.Forms.ComboBox()
        Me.btnDisconnectInlet1 = New System.Windows.Forms.Button()
        Me.btnCreateInlet1 = New System.Windows.Forms.Button()
        Me.lblInlet2 = New System.Windows.Forms.Label()
        Me.cbInlet2 = New System.Windows.Forms.ComboBox()
        Me.btnDisconnectInlet2 = New System.Windows.Forms.Button()
        Me.btnCreateInlet2 = New System.Windows.Forms.Button()
        Me.lblInlet3 = New System.Windows.Forms.Label()
        Me.cbInlet3 = New System.Windows.Forms.ComboBox()
        Me.btnDisconnectInlet3 = New System.Windows.Forms.Button()
        Me.btnCreateInlet3 = New System.Windows.Forms.Button()
        Me.lblVaporOut = New System.Windows.Forms.Label()
        Me.cbVaporOut = New System.Windows.Forms.ComboBox()
        Me.btnDisconnectVaporOut = New System.Windows.Forms.Button()
        Me.btnCreateVaporOut = New System.Windows.Forms.Button()
        Me.lblLiquidOut = New System.Windows.Forms.Label()
        Me.cbLiquidOut = New System.Windows.Forms.ComboBox()
        Me.btnDisconnectLiquidOut = New System.Windows.Forms.Button()
        Me.btnCreateLiquidOut = New System.Windows.Forms.Button()
        Me.lblEnergy = New System.Windows.Forms.Label()
        Me.cbEnergy = New System.Windows.Forms.ComboBox()
        Me.btnDisconnectEnergy = New System.Windows.Forms.Button()
        Me.btnCreateEnergy = New System.Windows.Forms.Button()

        ' GroupBox for Parameters with TabControl
        Me.GroupBoxParameters = New System.Windows.Forms.GroupBox()
        Me.TabControlParams = New System.Windows.Forms.TabControl()
        Me.TabPageCalc = New System.Windows.Forms.TabPage()
        Me.TabPageSizing = New System.Windows.Forms.TabPage()
        Me.TabPageResults = New System.Windows.Forms.TabPage()

        ' Calculation Tab Controls
        Me.lblCalcMode = New System.Windows.Forms.Label()
        Me.cbCalcMode = New System.Windows.Forms.ComboBox()
        Me.lblPressureCalc = New System.Windows.Forms.Label()
        Me.cbPressureCalc = New System.Windows.Forms.ComboBox()
        Me.chkOverrideT = New System.Windows.Forms.CheckBox()
        Me.tbFlashTemperature = New System.Windows.Forms.TextBox()
        Me.lblTempUnits = New System.Windows.Forms.Label()
        Me.chkOverrideP = New System.Windows.Forms.CheckBox()
        Me.tbFlashPressure = New System.Windows.Forms.TextBox()
        Me.lblPressUnits = New System.Windows.Forms.Label()

        ' Sizing Tab Controls
        Me.lblDimensionRatio = New System.Windows.Forms.Label()
        Me.tbDimensionRatio = New System.Windows.Forms.TextBox()
        Me.lblSurgeFactor = New System.Windows.Forms.Label()
        Me.tbSurgeFactor = New System.Windows.Forms.TextBox()
        Me.lblResidenceTime = New System.Windows.Forms.Label()
        Me.tbResidenceTime = New System.Windows.Forms.TextBox()
        Me.lblDiameter = New System.Windows.Forms.Label()
        Me.lblDiameterValue = New System.Windows.Forms.Label()
        Me.lblHeight = New System.Windows.Forms.Label()
        Me.lblHeightValue = New System.Windows.Forms.Label()

        ' Results Tab Controls
        Me.lblResultsTitle = New System.Windows.Forms.Label()
        Me.lblDeltaQ = New System.Windows.Forms.Label()
        Me.lblDeltaQValue = New System.Windows.Forms.Label()
        Me.lblVaporFlow = New System.Windows.Forms.Label()
        Me.lblVaporFlowValue = New System.Windows.Forms.Label()
        Me.lblLiquidFlow = New System.Windows.Forms.Label()
        Me.lblLiquidFlowValue = New System.Windows.Forms.Label()
        Me.lblVaporFraction = New System.Windows.Forms.Label()
        Me.lblVaporFractionValue = New System.Windows.Forms.Label()

        ' Suspend layouts
        Me.GroupBoxStatus.SuspendLayout()
        Me.GroupBoxConnections.SuspendLayout()
        Me.GroupBoxParameters.SuspendLayout()
        Me.TabControlParams.SuspendLayout()
        Me.TabPageCalc.SuspendLayout()
        Me.TabPageSizing.SuspendLayout()
        Me.TabPageResults.SuspendLayout()
        Me.SuspendLayout()

        ' ===============================
        ' GroupBoxStatus
        ' ===============================
        Me.GroupBoxStatus.Controls.Add(Me.lblTagLabel)
        Me.GroupBoxStatus.Controls.Add(Me.tbTag)
        Me.GroupBoxStatus.Controls.Add(Me.chkActive)
        Me.GroupBoxStatus.Controls.Add(Me.lblStatusLabel)
        Me.GroupBoxStatus.Controls.Add(Me.lblStatus)
        Me.GroupBoxStatus.Dock = System.Windows.Forms.DockStyle.Top
        Me.GroupBoxStatus.Location = New System.Drawing.Point(0, 0)
        Me.GroupBoxStatus.Name = "GroupBoxStatus"
        Me.GroupBoxStatus.Size = New System.Drawing.Size(384, 70)
        Me.GroupBoxStatus.TabIndex = 0
        Me.GroupBoxStatus.TabStop = False
        Me.GroupBoxStatus.Text = "Object"

        ' lblTagLabel
        Me.lblTagLabel.AutoSize = True
        Me.lblTagLabel.Location = New System.Drawing.Point(10, 22)
        Me.lblTagLabel.Name = "lblTagLabel"
        Me.lblTagLabel.Size = New System.Drawing.Size(38, 13)
        Me.lblTagLabel.Text = "Name:"

        ' tbTag
        Me.tbTag.Location = New System.Drawing.Point(55, 19)
        Me.tbTag.Name = "tbTag"
        Me.tbTag.Size = New System.Drawing.Size(150, 20)
        Me.tbTag.TabIndex = 1

        ' chkActive
        Me.chkActive.AutoSize = True
        Me.chkActive.Location = New System.Drawing.Point(220, 21)
        Me.chkActive.Name = "chkActive"
        Me.chkActive.Size = New System.Drawing.Size(56, 17)
        Me.chkActive.TabIndex = 2
        Me.chkActive.Text = "Active"

        ' lblStatusLabel
        Me.lblStatusLabel.AutoSize = True
        Me.lblStatusLabel.Location = New System.Drawing.Point(10, 47)
        Me.lblStatusLabel.Name = "lblStatusLabel"
        Me.lblStatusLabel.Size = New System.Drawing.Size(40, 13)
        Me.lblStatusLabel.Text = "Status:"

        ' lblStatus
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Location = New System.Drawing.Point(55, 47)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(78, 13)
        Me.lblStatus.Text = "Not Calculated"

        ' ===============================
        ' GroupBoxConnections
        ' ===============================
        Me.GroupBoxConnections.Controls.Add(Me.lblInlet1)
        Me.GroupBoxConnections.Controls.Add(Me.cbInlet1)
        Me.GroupBoxConnections.Controls.Add(Me.btnDisconnectInlet1)
        Me.GroupBoxConnections.Controls.Add(Me.btnCreateInlet1)
        Me.GroupBoxConnections.Controls.Add(Me.lblInlet2)
        Me.GroupBoxConnections.Controls.Add(Me.cbInlet2)
        Me.GroupBoxConnections.Controls.Add(Me.btnDisconnectInlet2)
        Me.GroupBoxConnections.Controls.Add(Me.btnCreateInlet2)
        Me.GroupBoxConnections.Controls.Add(Me.lblInlet3)
        Me.GroupBoxConnections.Controls.Add(Me.cbInlet3)
        Me.GroupBoxConnections.Controls.Add(Me.btnDisconnectInlet3)
        Me.GroupBoxConnections.Controls.Add(Me.btnCreateInlet3)
        Me.GroupBoxConnections.Controls.Add(Me.lblVaporOut)
        Me.GroupBoxConnections.Controls.Add(Me.cbVaporOut)
        Me.GroupBoxConnections.Controls.Add(Me.btnDisconnectVaporOut)
        Me.GroupBoxConnections.Controls.Add(Me.btnCreateVaporOut)
        Me.GroupBoxConnections.Controls.Add(Me.lblLiquidOut)
        Me.GroupBoxConnections.Controls.Add(Me.cbLiquidOut)
        Me.GroupBoxConnections.Controls.Add(Me.btnDisconnectLiquidOut)
        Me.GroupBoxConnections.Controls.Add(Me.btnCreateLiquidOut)
        Me.GroupBoxConnections.Controls.Add(Me.lblEnergy)
        Me.GroupBoxConnections.Controls.Add(Me.cbEnergy)
        Me.GroupBoxConnections.Controls.Add(Me.btnDisconnectEnergy)
        Me.GroupBoxConnections.Controls.Add(Me.btnCreateEnergy)
        Me.GroupBoxConnections.Dock = System.Windows.Forms.DockStyle.Top
        Me.GroupBoxConnections.Location = New System.Drawing.Point(0, 70)
        Me.GroupBoxConnections.Name = "GroupBoxConnections"
        Me.GroupBoxConnections.Size = New System.Drawing.Size(384, 190)
        Me.GroupBoxConnections.TabIndex = 1
        Me.GroupBoxConnections.TabStop = False
        Me.GroupBoxConnections.Text = "Connections"

        ' Row heights and positions
        Dim rowHeight As Integer = 26
        Dim startY As Integer = 20
        Dim lblX As Integer = 10
        Dim cbX As Integer = 75
        Dim cbWidth As Integer = 180
        Dim btnDisX As Integer = 262
        Dim btnCreateX As Integer = 310
        Dim btnWidth As Integer = 45

        ' Inlet 1
        Me.lblInlet1.AutoSize = True
        Me.lblInlet1.Location = New System.Drawing.Point(lblX, startY + 3)
        Me.lblInlet1.Name = "lblInlet1"
        Me.lblInlet1.Text = "Inlet 1:"

        Me.cbInlet1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbInlet1.Location = New System.Drawing.Point(cbX, startY)
        Me.cbInlet1.Name = "cbInlet1"
        Me.cbInlet1.Size = New System.Drawing.Size(cbWidth, 21)

        Me.btnDisconnectInlet1.Location = New System.Drawing.Point(btnDisX, startY)
        Me.btnDisconnectInlet1.Name = "btnDisconnectInlet1"
        Me.btnDisconnectInlet1.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnDisconnectInlet1.Text = "X"
        Me.btnDisconnectInlet1.UseVisualStyleBackColor = True

        Me.btnCreateInlet1.Location = New System.Drawing.Point(btnCreateX, startY)
        Me.btnCreateInlet1.Name = "btnCreateInlet1"
        Me.btnCreateInlet1.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnCreateInlet1.Text = "New"
        Me.btnCreateInlet1.UseVisualStyleBackColor = True

        ' Inlet 2
        startY += rowHeight
        Me.lblInlet2.AutoSize = True
        Me.lblInlet2.Location = New System.Drawing.Point(lblX, startY + 3)
        Me.lblInlet2.Name = "lblInlet2"
        Me.lblInlet2.Text = "Inlet 2:"

        Me.cbInlet2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbInlet2.Location = New System.Drawing.Point(cbX, startY)
        Me.cbInlet2.Name = "cbInlet2"
        Me.cbInlet2.Size = New System.Drawing.Size(cbWidth, 21)

        Me.btnDisconnectInlet2.Location = New System.Drawing.Point(btnDisX, startY)
        Me.btnDisconnectInlet2.Name = "btnDisconnectInlet2"
        Me.btnDisconnectInlet2.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnDisconnectInlet2.Text = "X"
        Me.btnDisconnectInlet2.UseVisualStyleBackColor = True

        Me.btnCreateInlet2.Location = New System.Drawing.Point(btnCreateX, startY)
        Me.btnCreateInlet2.Name = "btnCreateInlet2"
        Me.btnCreateInlet2.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnCreateInlet2.Text = "New"
        Me.btnCreateInlet2.UseVisualStyleBackColor = True

        ' Inlet 3
        startY += rowHeight
        Me.lblInlet3.AutoSize = True
        Me.lblInlet3.Location = New System.Drawing.Point(lblX, startY + 3)
        Me.lblInlet3.Name = "lblInlet3"
        Me.lblInlet3.Text = "Inlet 3:"

        Me.cbInlet3.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbInlet3.Location = New System.Drawing.Point(cbX, startY)
        Me.cbInlet3.Name = "cbInlet3"
        Me.cbInlet3.Size = New System.Drawing.Size(cbWidth, 21)

        Me.btnDisconnectInlet3.Location = New System.Drawing.Point(btnDisX, startY)
        Me.btnDisconnectInlet3.Name = "btnDisconnectInlet3"
        Me.btnDisconnectInlet3.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnDisconnectInlet3.Text = "X"
        Me.btnDisconnectInlet3.UseVisualStyleBackColor = True

        Me.btnCreateInlet3.Location = New System.Drawing.Point(btnCreateX, startY)
        Me.btnCreateInlet3.Name = "btnCreateInlet3"
        Me.btnCreateInlet3.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnCreateInlet3.Text = "New"
        Me.btnCreateInlet3.UseVisualStyleBackColor = True

        ' Vapor Out
        startY += rowHeight
        Me.lblVaporOut.AutoSize = True
        Me.lblVaporOut.Location = New System.Drawing.Point(lblX, startY + 3)
        Me.lblVaporOut.Name = "lblVaporOut"
        Me.lblVaporOut.Text = "Vapor:"

        Me.cbVaporOut.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbVaporOut.Location = New System.Drawing.Point(cbX, startY)
        Me.cbVaporOut.Name = "cbVaporOut"
        Me.cbVaporOut.Size = New System.Drawing.Size(cbWidth, 21)

        Me.btnDisconnectVaporOut.Location = New System.Drawing.Point(btnDisX, startY)
        Me.btnDisconnectVaporOut.Name = "btnDisconnectVaporOut"
        Me.btnDisconnectVaporOut.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnDisconnectVaporOut.Text = "X"
        Me.btnDisconnectVaporOut.UseVisualStyleBackColor = True

        Me.btnCreateVaporOut.Location = New System.Drawing.Point(btnCreateX, startY)
        Me.btnCreateVaporOut.Name = "btnCreateVaporOut"
        Me.btnCreateVaporOut.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnCreateVaporOut.Text = "New"
        Me.btnCreateVaporOut.UseVisualStyleBackColor = True

        ' Liquid Out
        startY += rowHeight
        Me.lblLiquidOut.AutoSize = True
        Me.lblLiquidOut.Location = New System.Drawing.Point(lblX, startY + 3)
        Me.lblLiquidOut.Name = "lblLiquidOut"
        Me.lblLiquidOut.Text = "Liquid:"

        Me.cbLiquidOut.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbLiquidOut.Location = New System.Drawing.Point(cbX, startY)
        Me.cbLiquidOut.Name = "cbLiquidOut"
        Me.cbLiquidOut.Size = New System.Drawing.Size(cbWidth, 21)

        Me.btnDisconnectLiquidOut.Location = New System.Drawing.Point(btnDisX, startY)
        Me.btnDisconnectLiquidOut.Name = "btnDisconnectLiquidOut"
        Me.btnDisconnectLiquidOut.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnDisconnectLiquidOut.Text = "X"
        Me.btnDisconnectLiquidOut.UseVisualStyleBackColor = True

        Me.btnCreateLiquidOut.Location = New System.Drawing.Point(btnCreateX, startY)
        Me.btnCreateLiquidOut.Name = "btnCreateLiquidOut"
        Me.btnCreateLiquidOut.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnCreateLiquidOut.Text = "New"
        Me.btnCreateLiquidOut.UseVisualStyleBackColor = True

        ' Energy
        startY += rowHeight
        Me.lblEnergy.AutoSize = True
        Me.lblEnergy.Location = New System.Drawing.Point(lblX, startY + 3)
        Me.lblEnergy.Name = "lblEnergy"
        Me.lblEnergy.Text = "Energy:"

        Me.cbEnergy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbEnergy.Location = New System.Drawing.Point(cbX, startY)
        Me.cbEnergy.Name = "cbEnergy"
        Me.cbEnergy.Size = New System.Drawing.Size(cbWidth, 21)

        Me.btnDisconnectEnergy.Location = New System.Drawing.Point(btnDisX, startY)
        Me.btnDisconnectEnergy.Name = "btnDisconnectEnergy"
        Me.btnDisconnectEnergy.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnDisconnectEnergy.Text = "X"
        Me.btnDisconnectEnergy.UseVisualStyleBackColor = True

        Me.btnCreateEnergy.Location = New System.Drawing.Point(btnCreateX, startY)
        Me.btnCreateEnergy.Name = "btnCreateEnergy"
        Me.btnCreateEnergy.Size = New System.Drawing.Size(btnWidth, 23)
        Me.btnCreateEnergy.Text = "New"
        Me.btnCreateEnergy.UseVisualStyleBackColor = True

        ' ===============================
        ' GroupBoxParameters with TabControl
        ' ===============================
        Me.GroupBoxParameters.Controls.Add(Me.TabControlParams)
        Me.GroupBoxParameters.Dock = System.Windows.Forms.DockStyle.Fill
        Me.GroupBoxParameters.Location = New System.Drawing.Point(0, 260)
        Me.GroupBoxParameters.Name = "GroupBoxParameters"
        Me.GroupBoxParameters.Size = New System.Drawing.Size(384, 282)
        Me.GroupBoxParameters.TabIndex = 2
        Me.GroupBoxParameters.TabStop = False
        Me.GroupBoxParameters.Text = "Parameters"

        ' TabControl
        Me.TabControlParams.Controls.Add(Me.TabPageCalc)
        Me.TabControlParams.Controls.Add(Me.TabPageSizing)
        Me.TabControlParams.Controls.Add(Me.TabPageResults)
        Me.TabControlParams.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabControlParams.Location = New System.Drawing.Point(3, 16)
        Me.TabControlParams.Name = "TabControlParams"
        Me.TabControlParams.SelectedIndex = 0
        Me.TabControlParams.Size = New System.Drawing.Size(378, 263)

        ' ===============================
        ' TabPageCalc - Calculation Settings
        ' ===============================
        Me.TabPageCalc.Controls.Add(Me.lblCalcMode)
        Me.TabPageCalc.Controls.Add(Me.cbCalcMode)
        Me.TabPageCalc.Controls.Add(Me.lblPressureCalc)
        Me.TabPageCalc.Controls.Add(Me.cbPressureCalc)
        Me.TabPageCalc.Controls.Add(Me.chkOverrideT)
        Me.TabPageCalc.Controls.Add(Me.tbFlashTemperature)
        Me.TabPageCalc.Controls.Add(Me.lblTempUnits)
        Me.TabPageCalc.Controls.Add(Me.chkOverrideP)
        Me.TabPageCalc.Controls.Add(Me.tbFlashPressure)
        Me.TabPageCalc.Controls.Add(Me.lblPressUnits)
        Me.TabPageCalc.Location = New System.Drawing.Point(4, 22)
        Me.TabPageCalc.Name = "TabPageCalc"
        Me.TabPageCalc.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageCalc.Size = New System.Drawing.Size(370, 237)
        Me.TabPageCalc.TabIndex = 0
        Me.TabPageCalc.Text = "Calculation"
        Me.TabPageCalc.UseVisualStyleBackColor = True

        ' Calculation Mode
        Me.lblCalcMode.AutoSize = True
        Me.lblCalcMode.Location = New System.Drawing.Point(10, 15)
        Me.lblCalcMode.Name = "lblCalcMode"
        Me.lblCalcMode.Text = "Calculation Mode:"

        Me.cbCalcMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbCalcMode.Items.AddRange(New Object() {"Adiabatic", "Legacy"})
        Me.cbCalcMode.Location = New System.Drawing.Point(120, 12)
        Me.cbCalcMode.Name = "cbCalcMode"
        Me.cbCalcMode.Size = New System.Drawing.Size(150, 21)

        ' Pressure Calculation
        Me.lblPressureCalc.AutoSize = True
        Me.lblPressureCalc.Location = New System.Drawing.Point(10, 45)
        Me.lblPressureCalc.Name = "lblPressureCalc"
        Me.lblPressureCalc.Text = "Pressure Behavior:"

        Me.cbPressureCalc.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbPressureCalc.Items.AddRange(New Object() {"Average", "Maximum", "Minimum"})
        Me.cbPressureCalc.Location = New System.Drawing.Point(120, 42)
        Me.cbPressureCalc.Name = "cbPressureCalc"
        Me.cbPressureCalc.Size = New System.Drawing.Size(150, 21)

        ' Override Temperature
        Me.chkOverrideT.AutoSize = True
        Me.chkOverrideT.Location = New System.Drawing.Point(10, 80)
        Me.chkOverrideT.Name = "chkOverrideT"
        Me.chkOverrideT.Size = New System.Drawing.Size(122, 17)
        Me.chkOverrideT.Text = "Override Temperature:"

        Me.tbFlashTemperature.Enabled = False
        Me.tbFlashTemperature.Location = New System.Drawing.Point(140, 78)
        Me.tbFlashTemperature.Name = "tbFlashTemperature"
        Me.tbFlashTemperature.Size = New System.Drawing.Size(80, 20)

        Me.lblTempUnits.AutoSize = True
        Me.lblTempUnits.Location = New System.Drawing.Point(225, 81)
        Me.lblTempUnits.Name = "lblTempUnits"
        Me.lblTempUnits.Text = "K"

        ' Override Pressure
        Me.chkOverrideP.AutoSize = True
        Me.chkOverrideP.Location = New System.Drawing.Point(10, 110)
        Me.chkOverrideP.Name = "chkOverrideP"
        Me.chkOverrideP.Size = New System.Drawing.Size(110, 17)
        Me.chkOverrideP.Text = "Override Pressure:"

        Me.tbFlashPressure.Enabled = False
        Me.tbFlashPressure.Location = New System.Drawing.Point(140, 108)
        Me.tbFlashPressure.Name = "tbFlashPressure"
        Me.tbFlashPressure.Size = New System.Drawing.Size(80, 20)

        Me.lblPressUnits.AutoSize = True
        Me.lblPressUnits.Location = New System.Drawing.Point(225, 111)
        Me.lblPressUnits.Name = "lblPressUnits"
        Me.lblPressUnits.Text = "Pa"

        ' ===============================
        ' TabPageSizing - Sizing Parameters
        ' ===============================
        Me.TabPageSizing.Controls.Add(Me.lblDimensionRatio)
        Me.TabPageSizing.Controls.Add(Me.tbDimensionRatio)
        Me.TabPageSizing.Controls.Add(Me.lblSurgeFactor)
        Me.TabPageSizing.Controls.Add(Me.tbSurgeFactor)
        Me.TabPageSizing.Controls.Add(Me.lblResidenceTime)
        Me.TabPageSizing.Controls.Add(Me.tbResidenceTime)
        Me.TabPageSizing.Controls.Add(Me.lblDiameter)
        Me.TabPageSizing.Controls.Add(Me.lblDiameterValue)
        Me.TabPageSizing.Controls.Add(Me.lblHeight)
        Me.TabPageSizing.Controls.Add(Me.lblHeightValue)
        Me.TabPageSizing.Location = New System.Drawing.Point(4, 22)
        Me.TabPageSizing.Name = "TabPageSizing"
        Me.TabPageSizing.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageSizing.Size = New System.Drawing.Size(370, 237)
        Me.TabPageSizing.TabIndex = 1
        Me.TabPageSizing.Text = "Sizing"
        Me.TabPageSizing.UseVisualStyleBackColor = True

        ' Dimension Ratio
        Me.lblDimensionRatio.AutoSize = True
        Me.lblDimensionRatio.Location = New System.Drawing.Point(10, 15)
        Me.lblDimensionRatio.Name = "lblDimensionRatio"
        Me.lblDimensionRatio.Text = "L/D Ratio:"

        Me.tbDimensionRatio.Location = New System.Drawing.Point(120, 12)
        Me.tbDimensionRatio.Name = "tbDimensionRatio"
        Me.tbDimensionRatio.Size = New System.Drawing.Size(80, 20)

        ' Surge Factor
        Me.lblSurgeFactor.AutoSize = True
        Me.lblSurgeFactor.Location = New System.Drawing.Point(10, 45)
        Me.lblSurgeFactor.Name = "lblSurgeFactor"
        Me.lblSurgeFactor.Text = "Surge Factor:"

        Me.tbSurgeFactor.Location = New System.Drawing.Point(120, 42)
        Me.tbSurgeFactor.Name = "tbSurgeFactor"
        Me.tbSurgeFactor.Size = New System.Drawing.Size(80, 20)

        ' Residence Time
        Me.lblResidenceTime.AutoSize = True
        Me.lblResidenceTime.Location = New System.Drawing.Point(10, 75)
        Me.lblResidenceTime.Name = "lblResidenceTime"
        Me.lblResidenceTime.Text = "Residence Time (min):"

        Me.tbResidenceTime.Location = New System.Drawing.Point(140, 72)
        Me.tbResidenceTime.Name = "tbResidenceTime"
        Me.tbResidenceTime.Size = New System.Drawing.Size(80, 20)

        ' Calculated Diameter (read-only)
        Me.lblDiameter.AutoSize = True
        Me.lblDiameter.Location = New System.Drawing.Point(10, 115)
        Me.lblDiameter.Name = "lblDiameter"
        Me.lblDiameter.Text = "Calculated Diameter:"
        Me.lblDiameter.Font = New System.Drawing.Font(Me.lblDiameter.Font, System.Drawing.FontStyle.Bold)

        Me.lblDiameterValue.AutoSize = True
        Me.lblDiameterValue.Location = New System.Drawing.Point(140, 115)
        Me.lblDiameterValue.Name = "lblDiameterValue"
        Me.lblDiameterValue.Text = "—"

        ' Calculated Height (read-only)
        Me.lblHeight.AutoSize = True
        Me.lblHeight.Location = New System.Drawing.Point(10, 140)
        Me.lblHeight.Name = "lblHeight"
        Me.lblHeight.Text = "Calculated Height:"
        Me.lblHeight.Font = New System.Drawing.Font(Me.lblHeight.Font, System.Drawing.FontStyle.Bold)

        Me.lblHeightValue.AutoSize = True
        Me.lblHeightValue.Location = New System.Drawing.Point(140, 140)
        Me.lblHeightValue.Name = "lblHeightValue"
        Me.lblHeightValue.Text = "—"

        ' ===============================
        ' TabPageResults - Calculation Results
        ' ===============================
        Me.TabPageResults.Controls.Add(Me.lblResultsTitle)
        Me.TabPageResults.Controls.Add(Me.lblDeltaQ)
        Me.TabPageResults.Controls.Add(Me.lblDeltaQValue)
        Me.TabPageResults.Controls.Add(Me.lblVaporFlow)
        Me.TabPageResults.Controls.Add(Me.lblVaporFlowValue)
        Me.TabPageResults.Controls.Add(Me.lblLiquidFlow)
        Me.TabPageResults.Controls.Add(Me.lblLiquidFlowValue)
        Me.TabPageResults.Controls.Add(Me.lblVaporFraction)
        Me.TabPageResults.Controls.Add(Me.lblVaporFractionValue)
        Me.TabPageResults.Location = New System.Drawing.Point(4, 22)
        Me.TabPageResults.Name = "TabPageResults"
        Me.TabPageResults.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPageResults.Size = New System.Drawing.Size(370, 237)
        Me.TabPageResults.TabIndex = 2
        Me.TabPageResults.Text = "Results"
        Me.TabPageResults.UseVisualStyleBackColor = True

        ' Results Title
        Me.lblResultsTitle.AutoSize = True
        Me.lblResultsTitle.Font = New System.Drawing.Font(Me.Font.FontFamily, 9, System.Drawing.FontStyle.Bold)
        Me.lblResultsTitle.Location = New System.Drawing.Point(10, 10)
        Me.lblResultsTitle.Name = "lblResultsTitle"
        Me.lblResultsTitle.Text = "Separation Results"

        ' Delta Q
        Me.lblDeltaQ.AutoSize = True
        Me.lblDeltaQ.Location = New System.Drawing.Point(10, 40)
        Me.lblDeltaQ.Name = "lblDeltaQ"
        Me.lblDeltaQ.Text = "Heat Duty (ΔQ):"

        Me.lblDeltaQValue.AutoSize = True
        Me.lblDeltaQValue.Location = New System.Drawing.Point(140, 40)
        Me.lblDeltaQValue.Name = "lblDeltaQValue"
        Me.lblDeltaQValue.Text = "—"

        ' Vapor Flow
        Me.lblVaporFlow.AutoSize = True
        Me.lblVaporFlow.Location = New System.Drawing.Point(10, 65)
        Me.lblVaporFlow.Name = "lblVaporFlow"
        Me.lblVaporFlow.Text = "Vapor Mass Flow:"

        Me.lblVaporFlowValue.AutoSize = True
        Me.lblVaporFlowValue.Location = New System.Drawing.Point(140, 65)
        Me.lblVaporFlowValue.Name = "lblVaporFlowValue"
        Me.lblVaporFlowValue.Text = "—"

        ' Liquid Flow
        Me.lblLiquidFlow.AutoSize = True
        Me.lblLiquidFlow.Location = New System.Drawing.Point(10, 90)
        Me.lblLiquidFlow.Name = "lblLiquidFlow"
        Me.lblLiquidFlow.Text = "Liquid Mass Flow:"

        Me.lblLiquidFlowValue.AutoSize = True
        Me.lblLiquidFlowValue.Location = New System.Drawing.Point(140, 90)
        Me.lblLiquidFlowValue.Name = "lblLiquidFlowValue"
        Me.lblLiquidFlowValue.Text = "—"

        ' Vapor Fraction
        Me.lblVaporFraction.AutoSize = True
        Me.lblVaporFraction.Location = New System.Drawing.Point(10, 115)
        Me.lblVaporFraction.Name = "lblVaporFraction"
        Me.lblVaporFraction.Text = "Vapor Fraction:"

        Me.lblVaporFractionValue.AutoSize = True
        Me.lblVaporFractionValue.Location = New System.Drawing.Point(140, 115)
        Me.lblVaporFractionValue.Name = "lblVaporFractionValue"
        Me.lblVaporFractionValue.Text = "—"

        ' ===============================
        ' Form Settings
        ' ===============================
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(384, 542)
        Me.Controls.Add(Me.GroupBoxParameters)
        Me.Controls.Add(Me.GroupBoxConnections)
        Me.Controls.Add(Me.GroupBoxStatus)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.Name = "EditingForm_GeothermalSeparator"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Geothermal Separator"

        ' Resume layouts
        Me.GroupBoxStatus.ResumeLayout(False)
        Me.GroupBoxStatus.PerformLayout()
        Me.GroupBoxConnections.ResumeLayout(False)
        Me.GroupBoxConnections.PerformLayout()
        Me.TabPageCalc.ResumeLayout(False)
        Me.TabPageCalc.PerformLayout()
        Me.TabPageSizing.ResumeLayout(False)
        Me.TabPageSizing.PerformLayout()
        Me.TabPageResults.ResumeLayout(False)
        Me.TabPageResults.PerformLayout()
        Me.TabControlParams.ResumeLayout(False)
        Me.GroupBoxParameters.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    ' Status GroupBox Controls
    Friend WithEvents GroupBoxStatus As System.Windows.Forms.GroupBox
    Friend WithEvents lblTagLabel As System.Windows.Forms.Label
    Friend WithEvents tbTag As System.Windows.Forms.TextBox
    Friend WithEvents chkActive As System.Windows.Forms.CheckBox
    Friend WithEvents lblStatusLabel As System.Windows.Forms.Label
    Friend WithEvents lblStatus As System.Windows.Forms.Label

    ' Connections GroupBox Controls
    Friend WithEvents GroupBoxConnections As System.Windows.Forms.GroupBox
    Friend WithEvents lblInlet1 As System.Windows.Forms.Label
    Friend WithEvents cbInlet1 As System.Windows.Forms.ComboBox
    Friend WithEvents btnDisconnectInlet1 As System.Windows.Forms.Button
    Friend WithEvents btnCreateInlet1 As System.Windows.Forms.Button
    Friend WithEvents lblInlet2 As System.Windows.Forms.Label
    Friend WithEvents cbInlet2 As System.Windows.Forms.ComboBox
    Friend WithEvents btnDisconnectInlet2 As System.Windows.Forms.Button
    Friend WithEvents btnCreateInlet2 As System.Windows.Forms.Button
    Friend WithEvents lblInlet3 As System.Windows.Forms.Label
    Friend WithEvents cbInlet3 As System.Windows.Forms.ComboBox
    Friend WithEvents btnDisconnectInlet3 As System.Windows.Forms.Button
    Friend WithEvents btnCreateInlet3 As System.Windows.Forms.Button
    Friend WithEvents lblVaporOut As System.Windows.Forms.Label
    Friend WithEvents cbVaporOut As System.Windows.Forms.ComboBox
    Friend WithEvents btnDisconnectVaporOut As System.Windows.Forms.Button
    Friend WithEvents btnCreateVaporOut As System.Windows.Forms.Button
    Friend WithEvents lblLiquidOut As System.Windows.Forms.Label
    Friend WithEvents cbLiquidOut As System.Windows.Forms.ComboBox
    Friend WithEvents btnDisconnectLiquidOut As System.Windows.Forms.Button
    Friend WithEvents btnCreateLiquidOut As System.Windows.Forms.Button
    Friend WithEvents lblEnergy As System.Windows.Forms.Label
    Friend WithEvents cbEnergy As System.Windows.Forms.ComboBox
    Friend WithEvents btnDisconnectEnergy As System.Windows.Forms.Button
    Friend WithEvents btnCreateEnergy As System.Windows.Forms.Button

    ' Parameters GroupBox and TabControl
    Friend WithEvents GroupBoxParameters As System.Windows.Forms.GroupBox
    Friend WithEvents TabControlParams As System.Windows.Forms.TabControl
    Friend WithEvents TabPageCalc As System.Windows.Forms.TabPage
    Friend WithEvents TabPageSizing As System.Windows.Forms.TabPage
    Friend WithEvents TabPageResults As System.Windows.Forms.TabPage

    ' Calculation Tab Controls
    Friend WithEvents lblCalcMode As System.Windows.Forms.Label
    Friend WithEvents cbCalcMode As System.Windows.Forms.ComboBox
    Friend WithEvents lblPressureCalc As System.Windows.Forms.Label
    Friend WithEvents cbPressureCalc As System.Windows.Forms.ComboBox
    Friend WithEvents chkOverrideT As System.Windows.Forms.CheckBox
    Friend WithEvents tbFlashTemperature As System.Windows.Forms.TextBox
    Friend WithEvents lblTempUnits As System.Windows.Forms.Label
    Friend WithEvents chkOverrideP As System.Windows.Forms.CheckBox
    Friend WithEvents tbFlashPressure As System.Windows.Forms.TextBox
    Friend WithEvents lblPressUnits As System.Windows.Forms.Label

    ' Sizing Tab Controls
    Friend WithEvents lblDimensionRatio As System.Windows.Forms.Label
    Friend WithEvents tbDimensionRatio As System.Windows.Forms.TextBox
    Friend WithEvents lblSurgeFactor As System.Windows.Forms.Label
    Friend WithEvents tbSurgeFactor As System.Windows.Forms.TextBox
    Friend WithEvents lblResidenceTime As System.Windows.Forms.Label
    Friend WithEvents tbResidenceTime As System.Windows.Forms.TextBox
    Friend WithEvents lblDiameter As System.Windows.Forms.Label
    Friend WithEvents lblDiameterValue As System.Windows.Forms.Label
    Friend WithEvents lblHeight As System.Windows.Forms.Label
    Friend WithEvents lblHeightValue As System.Windows.Forms.Label

    ' Results Tab Controls
    Friend WithEvents lblResultsTitle As System.Windows.Forms.Label
    Friend WithEvents lblDeltaQ As System.Windows.Forms.Label
    Friend WithEvents lblDeltaQValue As System.Windows.Forms.Label
    Friend WithEvents lblVaporFlow As System.Windows.Forms.Label
    Friend WithEvents lblVaporFlowValue As System.Windows.Forms.Label
    Friend WithEvents lblLiquidFlow As System.Windows.Forms.Label
    Friend WithEvents lblLiquidFlowValue As System.Windows.Forms.Label
    Friend WithEvents lblVaporFraction As System.Windows.Forms.Label
    Friend WithEvents lblVaporFractionValue As System.Windows.Forms.Label

End Class
