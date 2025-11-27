# Geothermal Separator UI Implementation Plan

## Problem Statement

When clicking on the Geothermal Separator vessel in DWSIM, a blank modal window appears instead of the docked property dialog that the default Gas-Liquid Separator (Vessel) shows.

### Root Cause Analysis

1. **`EditingForm_GeothermalSeparator.Designer.vb`** - Form is empty with no controls
2. **`EditingForm_GeothermalSeparator.vb`** - `UpdateInfo()` only sets the title, doesn't populate any UI
3. **`PopulateEditorPanel`** is empty (but this is OK - DWSIM calls `DisplayEditForm` instead)

The DWSIM Vessel properly implements a full editing form with:
- Stream connection management (6 inlets, 4 outlets, 1 energy)
- Property input controls (temperature, pressure, dimensions)
- Status display with calculation timestamps
- Tabbed interface for parameters/geometry/thermal
- Annotations panel

---

## Implementation Plan

### Phase 1: Form Designer - Add Controls

**File:** `EditingForms\EditingForm_GeothermalSeparator.Designer.vb`

#### 1.1 Main Container Structure

```
Form
├── GroupBox1 "Connections"
│   ├── Inlet Stream Controls (6 positions)
│   ├── Outlet Stream Controls (3 positions: Vapor, Liquid1, Liquid2)
│   └── Energy Stream Control
├── GroupBox2 "Parameters"
│   └── TabControl
│       ├── TabPage1 "Calculation"
│       ├── TabPage2 "Sizing"
│       └── TabPage3 "Annotations"
└── GroupBox3 "Status"
    ├── Tag Label
    ├── Status Label
    └── Active Checkbox
```

#### 1.2 Controls to Add

| Control Name | Type | Purpose | Location |
|-------------|------|---------|----------|
| **Connections GroupBox** ||||
| `lblInlet1-6` | Label | "Inlet 1:" through "Inlet 6:" | Left column |
| `cbInlet1-6` | ComboBox | Select inlet material stream | Middle |
| `btnDisconnectInlet1-6` | Button | Disconnect inlet | Right |
| `btnCreateInlet1-6` | Button | Create and connect new stream | Right |
| `lblVaporOut` | Label | "Vapor Out:" | Left column |
| `cbVaporOut` | ComboBox | Select vapor outlet stream | Middle |
| `btnDisconnectVaporOut` | Button | Disconnect vapor outlet | Right |
| `btnCreateVaporOut` | Button | Create vapor outlet stream | Right |
| `lblLiquidOut` | Label | "Liquid Out:" | Left column |
| `cbLiquidOut` | ComboBox | Select liquid outlet stream | Middle |
| `btnDisconnectLiquidOut` | Button | Disconnect liquid outlet | Right |
| `btnCreateLiquidOut` | Button | Create liquid outlet stream | Right |
| `lblLiquid2Out` | Label | "Liquid 2 Out:" | Left column |
| `cbLiquid2Out` | ComboBox | Select second liquid outlet | Middle |
| `btnDisconnectLiquid2Out` | Button | Disconnect second liquid outlet | Right |
| `btnCreateLiquid2Out` | Button | Create second liquid outlet | Right |
| `lblEnergy` | Label | "Energy:" | Left column |
| `cbEnergy` | ComboBox | Select energy stream | Middle |
| `btnDisconnectEnergy` | Button | Disconnect energy | Right |
| `btnCreateEnergy` | Button | Create energy stream | Right |
| **Parameters - Calculation Tab** ||||
| `lblCalcMode` | Label | "Calculation Mode:" | |
| `cbCalcMode` | ComboBox | Adiabatic/Legacy | |
| `lblPressureCalc` | Label | "Pressure Behavior:" | |
| `cbPressureCalc` | ComboBox | Minimum/Maximum/Average | |
| `chkOverrideT` | CheckBox | "Override Temperature" | |
| `tbFlashTemperature` | TextBox | Flash temperature value | |
| `cbTempUnits` | ComboBox | Temperature units (K, C, F) | |
| `chkOverrideP` | CheckBox | "Override Pressure" | |
| `tbFlashPressure` | TextBox | Flash pressure value | |
| `cbPressUnits` | ComboBox | Pressure units (Pa, kPa, bar) | |
| **Parameters - Sizing Tab** ||||
| `lblDimensionRatio` | Label | "L/D Ratio:" | |
| `tbDimensionRatio` | TextBox | Dimension ratio value | |
| `lblSurgeFactor` | Label | "Surge Factor:" | |
| `tbSurgeFactor` | TextBox | Surge factor value | |
| `lblResidenceTime` | Label | "Residence Time (min):" | |
| `tbResidenceTime` | TextBox | Residence time value | |
| `lblDiameter` | Label | "Calculated Diameter:" | |
| `lblDiameterValue` | Label | Display calculated DH | |
| `lblHeight` | Label | "Calculated Height:" | |
| `lblHeightValue` | Label | Display calculated AH | |
| **Parameters - Annotations Tab** ||||
| `rtbAnnotations` | RichTextBox | User notes | Fill tab |
| **Status GroupBox** ||||
| `lblTagLabel` | Label | "Name:" | |
| `tbTag` | TextBox | Editable unit name | |
| `chkActive` | CheckBox | "Active" | |
| `lblStatusLabel` | Label | "Status:" | |
| `lblStatus` | Label | Calculated/Not Calculated/Error | |

---

### Phase 2: Form Code - Implement Logic

**File:** `EditingForms\EditingForm_GeothermalSeparator.vb`

#### 2.1 Class Structure (Match DWSIM Vessel Pattern)

```vb
Imports System.Windows.Forms
Imports DWSIM.Interfaces.Enums.GraphicObjects
Imports su = DWSIM.SharedClasses.SystemsOfUnits
Imports WeifenLuo.WinFormsUI.Docking
Imports DWSIM.UnitOperations.UnitOperations

Public Class EditingForm_GeothermalSeparator
    Inherits SharedClasses.ObjectEditorForm

    Public Property SeparatorObject As GeothermalSeparator
    Public Loaded As Boolean = False

    Private units As SharedClasses.SystemsOfUnits.Units
    Private nf As String
```

#### 2.2 UpdateInfo() Method - Full Implementation

```vb
Public Sub UpdateInfo()
    If SeparatorObject Is Nothing Then Exit Sub

    units = SeparatorObject.FlowSheet.FlowsheetOptions.SelectedUnitSystem
    nf = SeparatorObject.FlowSheet.FlowsheetOptions.NumberFormat

    Loaded = False

    With SeparatorObject
        ' Update title
        Me.Text = .GraphicObject.Tag & " (" & .GetDisplayName() & ")"

        ' Update tag textbox
        tbTag.Text = .GraphicObject.Tag

        ' Update status
        UpdateStatus()

        ' Update active checkbox
        chkActive.Checked = .GraphicObject.Active

        ' Populate stream comboboxes
        PopulateStreamComboBoxes()

        ' Select currently connected streams
        SelectConnectedStreams()

        ' Update calculation mode
        cbCalcMode.SelectedIndex = CInt(.CalculationMode)
        cbPressureCalc.SelectedIndex = CInt(.PressureCalculation)

        ' Update override checkboxes
        chkOverrideT.Checked = .OverrideT
        chkOverrideP.Checked = .OverrideP

        ' Update flash conditions (with unit conversion)
        tbFlashTemperature.Text = SharedClasses.SystemsOfUnits.Converter.ConvertFromSI(
            units.temperature, .FlashTemperature).ToString(nf)
        tbFlashPressure.Text = SharedClasses.SystemsOfUnits.Converter.ConvertFromSI(
            units.pressure, .FlashPressure).ToString(nf)

        ' Enable/disable based on override status
        tbFlashTemperature.Enabled = .OverrideT
        tbFlashPressure.Enabled = .OverrideP

        ' Update sizing parameters
        tbDimensionRatio.Text = .DimensionRatio.ToString(nf)
        tbSurgeFactor.Text = .SurgeFactor.ToString(nf)
        tbResidenceTime.Text = .ResidenceTime.ToString(nf)

        ' Update calculated sizing results
        lblDiameterValue.Text = SharedClasses.SystemsOfUnits.Converter.ConvertFromSI(
            units.diameter, .DH).ToString(nf) & " " & units.diameter
        lblHeightValue.Text = SharedClasses.SystemsOfUnits.Converter.ConvertFromSI(
            units.distance, .AH).ToString(nf) & " " & units.distance
    End With

    Loaded = True
End Sub
```

#### 2.3 Stream Connection Methods

```vb
Private Sub PopulateStreamComboBoxes()
    ' Clear all comboboxes
    cbInlet1.Items.Clear()
    cbInlet2.Items.Clear()
    ' ... repeat for all inlet/outlet comboboxes

    ' Add empty option
    cbInlet1.Items.Add("")
    ' ... repeat

    ' Get all material streams from flowsheet
    For Each obj In SeparatorObject.FlowSheet.SimulationObjects.Values
        If TypeOf obj Is DWSIM.Thermodynamics.Streams.MaterialStream Then
            Dim ms = DirectCast(obj, DWSIM.Thermodynamics.Streams.MaterialStream)
            cbInlet1.Items.Add(ms.GraphicObject.Tag)
            cbInlet2.Items.Add(ms.GraphicObject.Tag)
            ' ... add to all material stream comboboxes
        ElseIf TypeOf obj Is DWSIM.Thermodynamics.Streams.EnergyStream Then
            Dim es = DirectCast(obj, DWSIM.Thermodynamics.Streams.EnergyStream)
            cbEnergy.Items.Add(es.GraphicObject.Tag)
        End If
    Next
End Sub

Private Sub SelectConnectedStreams()
    ' Check each inlet connector
    For i As Integer = 0 To 5
        Dim conn = SeparatorObject.GraphicObject.InputConnectors(i)
        If conn.IsAttached Then
            Dim streamTag = conn.AttachedConnector.AttachedFrom.Tag
            Select Case i
                Case 0 : cbInlet1.SelectedItem = streamTag
                Case 1 : cbInlet2.SelectedItem = streamTag
                ' ... etc
            End Select
        End If
    Next

    ' Check outlet connectors
    If SeparatorObject.GraphicObject.OutputConnectors(0).IsAttached Then
        cbVaporOut.SelectedItem = SeparatorObject.GraphicObject.OutputConnectors(0).AttachedConnector.AttachedTo.Tag
    End If
    ' ... etc
End Sub
```

#### 2.4 Event Handlers for Property Changes

```vb
Private Sub tbFlashTemperature_TextChanged(sender As Object, e As EventArgs) Handles tbFlashTemperature.TextChanged
    If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub

    Dim value As Double
    If Double.TryParse(tbFlashTemperature.Text, value) Then
        ' Convert from display units to SI
        SeparatorObject.FlashTemperature = SharedClasses.SystemsOfUnits.Converter.ConvertToSI(
            units.temperature, value)
        tbFlashTemperature.ForeColor = Drawing.Color.Blue
    Else
        tbFlashTemperature.ForeColor = Drawing.Color.Red
    End If
End Sub

Private Sub tbFlashTemperature_KeyDown(sender As Object, e As KeyEventArgs) Handles tbFlashTemperature.KeyDown
    If e.KeyCode = Keys.Enter Then
        RequestCalc()
    End If
End Sub

Private Sub chkOverrideT_CheckedChanged(sender As Object, e As EventArgs) Handles chkOverrideT.CheckedChanged
    If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
    SeparatorObject.OverrideT = chkOverrideT.Checked
    tbFlashTemperature.Enabled = chkOverrideT.Checked
End Sub

Private Sub cbCalcMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbCalcMode.SelectedIndexChanged
    If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
    SeparatorObject.CalculationMode = CType(cbCalcMode.SelectedIndex, GeothermalSeparator.CalculationModes)
End Sub
```

#### 2.5 Connection Event Handlers

```vb
Private Sub cbInlet1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbInlet1.SelectedIndexChanged
    If Not Loaded OrElse SeparatorObject Is Nothing Then Exit Sub
    UpdateInletConnection(0, cbInlet1.SelectedItem?.ToString())
End Sub

Private Sub UpdateInletConnection(index As Integer, streamTag As String)
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
            SeparatorObject.FlowSheet.ConnectObjects(
                stream.GraphicObject,
                SeparatorObject.GraphicObject,
                0, index)
        End If
    End If
End Sub

Private Sub btnDisconnectInlet1_Click(sender As Object, e As EventArgs) Handles btnDisconnectInlet1.Click
    If SeparatorObject Is Nothing Then Exit Sub

    Dim connector = SeparatorObject.GraphicObject.InputConnectors(0)
    If connector.IsAttached Then
        SeparatorObject.FlowSheet.DisconnectObjects(
            connector.AttachedConnector.AttachedFrom,
            SeparatorObject.GraphicObject)
        cbInlet1.SelectedIndex = -1
    End If
End Sub

Private Sub btnCreateInlet1_Click(sender As Object, e As EventArgs) Handles btnCreateInlet1.Click
    If SeparatorObject Is Nothing Then Exit Sub

    ' Create new material stream
    Dim ms = SeparatorObject.FlowSheet.AddObject(
        ObjectType.MaterialStream,
        SeparatorObject.GraphicObject.X - 100,
        SeparatorObject.GraphicObject.InputConnectors(0).Position.Y,
        SeparatorObject.GraphicObject.Tag & "_In1")

    ' Connect it
    SeparatorObject.FlowSheet.ConnectObjects(
        ms.GraphicObject,
        SeparatorObject.GraphicObject,
        0, 0)

    UpdateInfo()
End Sub
```

#### 2.6 Status Update Method

```vb
Private Sub UpdateStatus()
    If SeparatorObject.GraphicObject.Calculated Then
        lblStatus.Text = "Calculated"
        lblStatus.ForeColor = Drawing.Color.Blue
    ElseIf Not SeparatorObject.GraphicObject.Active Then
        lblStatus.Text = "Inactive"
        lblStatus.ForeColor = Drawing.Color.Gray
    Else
        lblStatus.Text = "Not Calculated"
        lblStatus.ForeColor = Drawing.Color.Black
    End If
End Sub
```

---

### Phase 3: Unit Operation Updates

**File:** `UnitOperations\GeothermalSeparator.vb`

#### 3.1 Update DisplayEditForm (Minor Fix)

Current implementation is mostly correct but needs to use `GlobalSettings.Settings.DefaultEditFormLocation`:

```vb
Public Overrides Sub DisplayEditForm()
    If f Is Nothing Then
        f = New EditingForm_GeothermalSeparator With {.SeparatorObject = Me}
        f.ShowHint = GlobalSettings.Settings.DefaultEditFormLocation  ' Use DWSIM default
        f.Tag = "ObjectEditor"
        Me.FlowSheet.DisplayForm(f)
    Else
        If f.IsDisposed Then
            f = New EditingForm_GeothermalSeparator With {.SeparatorObject = Me}
            f.ShowHint = GlobalSettings.Settings.DefaultEditFormLocation
            f.Tag = "ObjectEditor"
            Me.FlowSheet.DisplayForm(f)
        Else
            f.Activate()
        End If
    End If
End Sub
```

#### 3.2 Update UpdateEditForm (Add Thread Safety)

```vb
Public Overrides Sub UpdateEditForm()
    If f IsNot Nothing Then
        If Not f.IsDisposed Then
            f.UIThread(Sub() f.UpdateInfo())  ' Thread-safe update
        End If
    End If
End Sub
```

---

### Phase 4: Testing Checklist

#### 4.1 Basic Functionality
- [ ] Form opens when double-clicking unit in flowsheet
- [ ] Form docks to left panel (default DWSIM behavior)
- [ ] Tag is displayed and editable
- [ ] Status shows correctly (Calculated/Not Calculated/Inactive)
- [ ] Active checkbox toggles unit active state

#### 4.2 Stream Connections
- [ ] All inlet stream dropdowns populate with available material streams
- [ ] Energy stream dropdown populates with available energy streams
- [ ] Outlet stream dropdowns populate correctly
- [ ] Selecting a stream connects it
- [ ] Disconnect buttons work
- [ ] Create buttons create and connect new streams

#### 4.3 Properties
- [ ] Calculation mode dropdown works (Adiabatic/Legacy)
- [ ] Pressure behavior dropdown works (Min/Max/Avg)
- [ ] Override Temperature checkbox enables/disables temperature input
- [ ] Override Pressure checkbox enables/disables pressure input
- [ ] Temperature/Pressure values displayed in correct units
- [ ] Entering new values updates the unit operation
- [ ] Pressing Enter triggers recalculation

#### 4.4 Sizing
- [ ] Dimension ratio editable
- [ ] Surge factor editable
- [ ] Residence time editable
- [ ] Calculated diameter displays after calculation
- [ ] Calculated height displays after calculation

#### 4.5 Calculation Integration
- [ ] Changing properties triggers recalculation
- [ ] Results update after calculation completes
- [ ] Error states display correctly

---

### File Summary

| File | Action | Lines of Change |
|------|--------|-----------------|
| `EditingForm_GeothermalSeparator.Designer.vb` | Rewrite | ~400 lines |
| `EditingForm_GeothermalSeparator.vb` | Rewrite | ~350 lines |
| `GeothermalSeparator.vb` | Minor updates | ~10 lines |

### Dependencies

All dependencies are already in the project:
- `WeifenLuo.WinFormsUI.Docking` - For docking support
- `DWSIM.SharedClasses` - For `ObjectEditorForm` and `SystemsOfUnits`
- `System.Windows.Forms` - For UI controls

### Implementation Order

1. **Designer File First** - Add all controls with proper layout
2. **Code File Second** - Implement all event handlers and logic
3. **Unit Operation Last** - Minor updates for thread safety
4. **Test** - Verify all functionality

---

## Reference: DWSIM Vessel EditingForm

Source: [GitHub - DanWBR/dwsim](https://github.com/DanWBR/dwsim/blob/windows/DWSIM.UnitOperations/EditingForms/EditingForm_Vessel.vb)

The DWSIM Vessel uses this pattern which we are replicating:
- Inherits from `SharedClasses.ObjectEditorForm`
- Property `VesselObject As Vessel` to reference the unit operation
- `UpdateInfo()` method called on form load and after calculations
- Stream connections managed via ComboBox with disconnect/create buttons
- Properties validated on change, committed on Enter key
- Uses `UIThread()` for thread-safe UI updates

---

## Approval Required

Before implementation:
1. Review this plan for any missing features
2. Confirm the UI layout matches expectations
3. Verify all required properties are included

Once approved, implementation will follow the phases outlined above, with commits after each phase passes testing.
