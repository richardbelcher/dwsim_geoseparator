'    Geothermal Separator Calculation Routines
'    Based on DWSIM Vessel by Daniel Wagner O. de Medeiros
'    Modified for Lazalde-Crabtree geothermal separator design
'    Copyright 2025 MTL Geothermal
'
'    This file is part of DWSIM.
'
'    DWSIM is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    DWSIM is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with DWSIM.  If not, see <http://www.gnu.org/licenses/>.


Imports System.Windows.Forms
Imports DWSIM.Thermodynamics
Imports DWSIM.Thermodynamics.Streams
Imports DWSIM.SharedClasses
Imports DWSIM.Interfaces
Imports DWSIM.Interfaces.Enums
Imports SkiaSharp.Views.Desktop.Extensions

Namespace UnitOperations

    <System.Serializable()> Public Class GeothermalSeparator

        Inherits UnitOperations.UnitOpBaseClass
        Implements IExternalUnitOperation

        Public Overrides Property ObjectClass As SimulationObjectClass = SimulationObjectClass.Separators

        <NonSerialized> <Xml.Serialization.XmlIgnore> Public f As EditingForm_GeothermalSeparator

        <NonSerialized> <Xml.Serialization.XmlIgnore> Public MixedStream As MaterialStream

        Protected m_DQ As Nullable(Of Double)

        ' Sizing results
        Dim rhol, rhov, ql, qv, qe, rhoe, wl, wv As Double
        Public AH, DH As Double
        Public AV, DV As Double

        Public Enum PressureBehavior
            Average = 0
            Maximum = 1
            Minimum = 2
        End Enum

        Public Enum CalculationModes
            Adiabatic = 0
            Legacy = 1
        End Enum

        ''' <summary>
        ''' Separator equipment type: Separator (X_i less than 95%) or Dryer (X_i greater than 95%)
        ''' </summary>
        Public Enum SeparatorType
            Separator = 0  ' Inlet quality < 95%, rectangular spiral inlet
            Dryer = 1      ' Inlet quality > 95%, circular tangential inlet
        End Enum

        ''' <summary>
        ''' Sizing mode for separator design
        ''' </summary>
        Public Enum SizingModes
            AutoSize = 0   ' Calculate D_t from design velocity and steam flow
            Rating = 1     ' User specifies D_t, calculate actual performance
            Design = 2     ' User specifies target quality, iterate to find D_t
        End Enum

        ''' <summary>
        ''' Two-phase flow pattern (Baker's method)
        ''' </summary>
        Public Enum FlowPatterns
            Auto = 0           ' Automatically detect from Baker chart
            Stratified = 1     ' Stratified/Wavy flow
            Annular = 2        ' Annular flow
            Dispersed = 3      ' Dispersed/Bubble flow
            PlugSlug = 4       ' Plug/Slug flow
        End Enum

        Public Property CalculationMode As CalculationModes = CalculationModes.Adiabatic

        Public Property DimensionRatio As Double = 3.3

        Public Property SurgeFactor As Double = 1.2

        Public Property ResidenceTime As Double = 5

        Public Property PressureCalculation() As PressureBehavior = PressureBehavior.Minimum

        Public Property OverrideT As Boolean = False

        Public Property OverrideP As Boolean = False

        Public Property FlashPressure As Double = 101325

        Public Property FlashTemperature As Double = 298.15

        Public Property DeltaQ As Nullable(Of Double)

#Region "Lazalde-Crabtree Properties"

        ' === INPUT PROPERTIES ===

        ''' <summary>
        ''' Equipment type: Separator or Dryer
        ''' </summary>
        Public Property EquipmentType As SeparatorType = SeparatorType.Separator

        ''' <summary>
        ''' Sizing mode: AutoSize, Rating, or Design
        ''' </summary>
        Public Property SizingMode As SizingModes = SizingModes.AutoSize

        ''' <summary>
        ''' Two-phase flow pattern for drop diameter calculation
        ''' </summary>
        Public Property FlowPattern As FlowPatterns = FlowPatterns.Auto

        ''' <summary>
        ''' Inlet pipe diameter D_t [m] - used in Rating mode
        ''' </summary>
        Public Property InletPipeDiameter As Double = 0.3

        ''' <summary>
        ''' Design steam velocity V_T [m/s]
        ''' Recommended: 25-45 for separator, 35-60 for dryer
        ''' </summary>
        Public Property DesignSteamVelocity As Double = 35.0

        ''' <summary>
        ''' Target outlet steam quality (used in Design mode)
        ''' </summary>
        Public Property TargetOutletQuality As Double = 0.9995

        ' === DIMENSIONAL PROPERTIES (Calculated) ===

        ''' <summary>
        ''' Vessel diameter D [m] = 3.3×D_t (sep) or 3.5×D_t (dryer)
        ''' </summary>
        Public Property VesselDiameter As Double

        ''' <summary>
        ''' Steam outlet diameter D_e [m] = 1.0×D_t
        ''' </summary>
        Public Property SteamOutletDiameter As Double

        ''' <summary>
        ''' Water outlet diameter D_b [m] = 1.0×D_t
        ''' </summary>
        Public Property WaterOutletDiameter As Double

        ''' <summary>
        ''' Lip position alpha [m] = -0.15×D_t (negative = inside head)
        ''' </summary>
        Public Property LipPosition As Double

        ''' <summary>
        ''' Cyclone height beta [m] = 3.5×D_t (sep) or 3.0×D_t (dryer)
        ''' </summary>
        Public Property CycloneHeight As Double

        ''' <summary>
        ''' Total height Z [m] = 5.5×D_t (sep) or 4.0×D_t (dryer)
        ''' </summary>
        Public Property TotalHeight As Double

        ''' <summary>
        ''' Inlet area A_o [m²]
        ''' </summary>
        Public Property InletArea As Double

        ' === VELOCITY PROPERTIES (Calculated) ===

        ''' <summary>
        ''' Actual inlet steam velocity V_T [m/s]
        ''' </summary>
        Public Property ActualSteamVelocity As Double

        ''' <summary>
        ''' Inlet tangential velocity u [m/s]
        ''' </summary>
        Public Property TangentialVelocity As Double

        ''' <summary>
        ''' Annular upward velocity V_AN [m/s]
        ''' </summary>
        Public Property AnnularVelocity As Double

        ' === EFFICIENCY PROPERTIES (Calculated) ===

        ''' <summary>
        ''' Inlet steam quality X_i (mass fraction vapor)
        ''' </summary>
        Public Property InletSteamQuality As Double

        ''' <summary>
        ''' Drop diameter d_w [microns] - Nukiyama-Tanasawa
        ''' </summary>
        Public Property DropDiameter As Double

        ''' <summary>
        ''' Detected flow pattern from Baker chart
        ''' </summary>
        Public Property DetectedFlowPattern As FlowPatterns

        ''' <summary>
        ''' Centrifugal efficiency eta_m (0-1)
        ''' </summary>
        Public Property CentrifugalEfficiency As Double

        ''' <summary>
        ''' Entrainment efficiency eta_A (0-1)
        ''' </summary>
        Public Property EntrainmentEfficiency As Double

        ''' <summary>
        ''' Overall separation efficiency eta_ef = eta_m × eta_A (0-1)
        ''' </summary>
        Public Property OverallEfficiency As Double

        ''' <summary>
        ''' Outlet steam quality X_o (0-1)
        ''' </summary>
        Public Property OutletSteamQuality As Double

        ''' <summary>
        ''' Entrainment (water carryover) W_A [kg/s]
        ''' </summary>
        Public Property WaterCarryover As Double

        ''' <summary>
        ''' Separator pressure drop [Pa]
        ''' </summary>
        Public Property SeparatorPressureDrop As Double

        ''' <summary>
        ''' Number of velocity heads N_H for pressure drop
        ''' </summary>
        Public Property VelocityHeads As Double

        ' === VALIDATION/WARNING PROPERTIES ===

        ''' <summary>
        ''' Velocity status message
        ''' </summary>
        Public Property VelocityStatus As String = ""

        ''' <summary>
        ''' True if velocity is within recommended range
        ''' </summary>
        Public Property VelocityInRange As Boolean = True

#End Region

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal name As String, ByVal description As String)
            MyBase.CreateNew()
            Me.ComponentName = name
            Me.ComponentDescription = description
        End Sub

        Public Overrides Function CloneXML() As Object
            Dim obj As ICustomXMLSerialization = New GeothermalSeparator()
            obj.LoadData(Me.SaveData)
            Return obj
        End Function

        Public Overrides Function CloneJSON() As Object
            Return Newtonsoft.Json.JsonConvert.DeserializeObject(Of GeothermalSeparator)(Newtonsoft.Json.JsonConvert.SerializeObject(Me))
        End Function

        Public Overrides Sub Calculate(Optional ByVal args As Object = Nothing)

            ' Verify outlet connections
            If Not Me.GraphicObject.OutputConnectors(0).IsAttached Then
                Throw New Exception(FlowSheet.GetTranslatedString("Verifiqueasconexesdo"))
            ElseIf Not Me.GraphicObject.OutputConnectors(1).IsAttached Then
                Throw New Exception(FlowSheet.GetTranslatedString("Verifiqueasconexesdo"))
            End If

            Dim E0 As Double = 0.0#

            If OverrideP Or OverrideT Then CalculationMode = CalculationModes.Legacy

            Dim H, T, W, M, We, P, VF, Hf, H0 As Double, nstr As Integer
            H = 0 : T = 0 : W = 0 : We = 0 : P = 0 : VF = 0.0#

            Dim i As Integer = 1
            Dim nc As Integer = 0

            MixedStream = New MaterialStream("", "", Me.FlowSheet, Me.PropertyPackage)
            FlowSheet.AddCompoundsToMaterialStream(MixedStream)
            Dim ms As MaterialStream = Nothing

            Dim cp As IConnectionPoint

            nstr = 0
            For Each cp In Me.GraphicObject.InputConnectors
                If cp.IsAttached And cp.Type = GraphicObjects.ConType.ConIn Then
                    nc += 1
                    If cp.AttachedConnector.AttachedFrom.Calculated = False Then Throw New Exception(FlowSheet.GetTranslatedString("Umaoumaiscorrentesna"))
                    ms = FlowSheet.SimulationObjects(cp.AttachedConnector.AttachedFrom.Name)
                    ms.Validate()
                    If Me.PressureCalculation = PressureBehavior.Minimum Then
                        If ms.Phases(0).Properties.pressure.GetValueOrDefault < P Then
                            P = ms.Phases(0).Properties.pressure.GetValueOrDefault
                        ElseIf P = 0 Then
                            P = ms.Phases(0).Properties.pressure.GetValueOrDefault
                        End If
                    ElseIf Me.PressureCalculation = PressureBehavior.Maximum Then
                        If ms.Phases(0).Properties.pressure.GetValueOrDefault > P Then
                            P = ms.Phases(0).Properties.pressure.GetValueOrDefault
                        ElseIf P = 0 Then
                            P = ms.Phases(0).Properties.pressure.GetValueOrDefault
                        End If
                    Else
                        P = P + ms.Phases(0).Properties.pressure.GetValueOrDefault
                        i += 1
                    End If
                    M += ms.Phases(0).Properties.molarflow.GetValueOrDefault
                    We = ms.Phases(0).Properties.massflow.GetValueOrDefault
                    W += We
                    VF += ms.Phases(2).Properties.molarfraction.GetValueOrDefault * ms.Phases(0).Properties.molarflow.GetValueOrDefault
                    If Not Double.IsNaN(ms.Phases(0).Properties.enthalpy.GetValueOrDefault) Then H += We * ms.Phases(0).Properties.enthalpy.GetValueOrDefault
                    nstr += 1
                End If
            Next

            If M <> 0.0# Then VF /= M

            H0 = H

            If Me.PressureCalculation = PressureBehavior.Average Then P = P / (i - 1)

            T = 0

            Dim n As Integer = ms.Phases(0).Compounds.Count
            Dim Vw As New Dictionary(Of String, Double)
            For Each cp In Me.GraphicObject.InputConnectors
                If cp.IsAttached And cp.Type = GraphicObjects.ConType.ConIn Then
                    ms = FlowSheet.SimulationObjects(cp.AttachedConnector.AttachedFrom.Name)
                    Dim comp As BaseClasses.Compound
                    For Each comp In ms.Phases(0).Compounds.Values
                        If Not Vw.ContainsKey(comp.Name) Then
                            Vw.Add(comp.Name, 0)
                        End If
                        Vw(comp.Name) += comp.MassFraction.GetValueOrDefault * ms.Phases(0).Properties.massflow.GetValueOrDefault
                    Next
                    If W <> 0.0# Then T += ms.Phases(0).Properties.massflow.GetValueOrDefault / W * ms.Phases(0).Properties.temperature.GetValueOrDefault
                End If
            Next

            If W = 0.0# Then T = 273.15

            CheckSpec(H, False, "enthalpy")
            CheckSpec(W, True, "mass flow")
            CheckSpec(P, True, "pressure")

            With MixedStream

                .PreferredFlashAlgorithmTag = Me.PreferredFlashAlgorithmTag

                .Phases(0).Properties.enthalpy = H
                .Phases(0).Properties.pressure = P
                .Phases(0).Properties.massflow = W
                .Phases(0).Properties.molarfraction = 1
                .Phases(0).Properties.massfraction = 1
                .Phases(2).Properties.molarfraction = VF
                Dim comp As BaseClasses.Compound
                For Each comp In .Phases(0).Compounds.Values
                    If W <> 0.0# Then comp.MassFraction = Vw(comp.Name) / W
                Next
                Dim mass_div_mm As Double = 0
                Dim sub1 As BaseClasses.Compound
                For Each sub1 In .Phases(0).Compounds.Values
                    mass_div_mm += sub1.MassFraction.GetValueOrDefault / sub1.ConstantProperties.Molar_Weight
                Next
                For Each sub1 In .Phases(0).Compounds.Values
                    If W <> 0.0# Then
                        sub1.MoleFraction = sub1.MassFraction.GetValueOrDefault / sub1.ConstantProperties.Molar_Weight / mass_div_mm
                    Else
                        sub1.MoleFraction = 0.0#
                    End If
                Next
                Me.PropertyPackage.CurrentMaterialStream = MixedStream
                MixedStream.Phases(0).Properties.temperature = T
                .Phases(0).Properties.molarflow = W / Me.PropertyPackage.AUX_MMM(PropertyPackages.Phase.Mixture) * 1000

            End With

            Select Case CalculationMode

                Case CalculationModes.Adiabatic

                    W = MixedStream.Phases(0).Properties.massflow.GetValueOrDefault

                    If nstr = 1 And E0 = 0.0# Then
                        ' No need to perform flash if there's only one stream and no heat added
                        For Each cp In Me.GraphicObject.InputConnectors
                            If cp.IsAttached And cp.Type = GraphicObjects.ConType.ConIn Then
                                ms = FlowSheet.SimulationObjects(cp.AttachedConnector.AttachedFrom.Name)
                                MixedStream.Assign(ms)
                                MixedStream.AssignProps(ms)
                                Exit For
                            End If
                        Next
                    Else
                        MixedStream.PropertyPackage = Me.PropertyPackage
                        MixedStream.SpecType = StreamSpec.Pressure_and_Enthalpy
                        MixedStream.Calculate(True, True)
                    End If

                    T = MixedStream.Phases(0).Properties.temperature.GetValueOrDefault

                Case CalculationModes.Legacy

                    W = MixedStream.Phases(0).Properties.massflow.GetValueOrDefault

                    If Me.OverrideP Then
                        If Not Me.GraphicObject.InputConnectors(6).IsAttached Then Throw New Exception(FlowSheet.GetTranslatedString("EnergyStreamRequired"))
                        P = Me.FlashPressure
                        MixedStream.Phases(0).Properties.pressure = P
                    Else
                        P = MixedStream.Phases(0).Properties.pressure.GetValueOrDefault
                    End If
                    If Me.OverrideT Then
                        If Not Me.GraphicObject.InputConnectors(6).IsAttached Then Throw New Exception(FlowSheet.GetTranslatedString("EnergyStreamRequired"))
                        T = Me.FlashTemperature
                        MixedStream.Phases(0).Properties.temperature = T
                    Else
                        T = MixedStream.Phases(0).Properties.temperature.GetValueOrDefault
                    End If

                    Me.PropertyPackage.CurrentMaterialStream = MixedStream

                    MixedStream.PropertyPackage = Me.PropertyPackage
                    MixedStream.SpecType = StreamSpec.Temperature_and_Pressure
                    MixedStream.Calculate(True, True)

            End Select

            ' Calculate distribution of solids into liquid outlet streams
            Dim SR, VnL1(n - 1), VnL2(n - 1), VmL1(n - 1), VmL2(n - 1) As Double
            Dim HL1, HL2, W1, W2, WL1, WL2, WS As Double
            WL1 = MixedStream.Phases(3).Properties.massflow.GetValueOrDefault
            WL2 = MixedStream.Phases(4).Properties.massflow.GetValueOrDefault
            If WL2 > 0.0# Then
                SR = WL1 / (WL1 + WL2)
            Else
                SR = 1
            End If
            Dim Vids As New List(Of String)
            i = 0
            For Each comp In MixedStream.Phases(0).Compounds.Values
                VnL1(i) = MixedStream.Phases(3).Compounds(comp.Name).MolarFlow.GetValueOrDefault + SR * MixedStream.Phases(7).Compounds(comp.Name).MolarFlow.GetValueOrDefault
                VmL1(i) = MixedStream.Phases(3).Compounds(comp.Name).MassFlow.GetValueOrDefault + SR * MixedStream.Phases(7).Compounds(comp.Name).MassFlow.GetValueOrDefault
                VnL2(i) = MixedStream.Phases(4).Compounds(comp.Name).MolarFlow.GetValueOrDefault + (1 - SR) * MixedStream.Phases(7).Compounds(comp.Name).MolarFlow.GetValueOrDefault
                VmL2(i) = MixedStream.Phases(4).Compounds(comp.Name).MassFlow.GetValueOrDefault + (1 - SR) * MixedStream.Phases(7).Compounds(comp.Name).MassFlow.GetValueOrDefault
                Vids.Add(comp.Name)
                i += 1
            Next
            Dim sum1, sum2, sum3, sum4 As Double
            sum1 = VnL1.Sum
            If VnL1.Sum > 0.0# Then
                For i = 0 To VnL1.Length - 1
                    VnL1(i) /= sum1
                Next
            End If
            sum2 = VmL1.Sum
            If VmL1.Sum > 0.0# Then
                For i = 0 To VnL1.Length - 1
                    VmL1(i) /= sum2
                Next
            End If
            sum3 = VnL2.Sum
            If VnL2.Sum > 0.0# Then
                For i = 0 To VnL1.Length - 1
                    VnL2(i) /= sum3
                Next
            End If
            sum4 = VmL2.Sum
            If VmL2.Sum > 0.0# Then
                For i = 0 To VnL1.Length - 1
                    VmL2(i) /= sum4
                Next
            End If
            WL1 = MixedStream.Phases(3).Properties.massflow.GetValueOrDefault
            WL2 = MixedStream.Phases(4).Properties.massflow.GetValueOrDefault
            WS = MixedStream.Phases(7).Properties.massflow.GetValueOrDefault
            W1 = WL1 + SR * WS
            W2 = WL2 + (1 - SR) * WS
            HL1 = (WL1 * MixedStream.Phases(3).Properties.enthalpy.GetValueOrDefault + WS * SR * MixedStream.Phases(7).Properties.enthalpy.GetValueOrDefault) / (WL1 + WS * SR)
            HL2 = (WL2 * MixedStream.Phases(4).Properties.enthalpy.GetValueOrDefault + WS * (1 - SR) * MixedStream.Phases(7).Properties.enthalpy.GetValueOrDefault) / (WL2 + WS * (1 - SR))

            If Double.IsNaN(HL1) Then HL1 = 0.0#
            If Double.IsNaN(HL2) Then HL2 = 0.0#
            If Double.IsNaN(WL1) Then WL1 = 0.0#
            If Double.IsNaN(WL2) Then WL2 = 0.0#

            ' Vapour phase output
            cp = Me.GraphicObject.OutputConnectors(0)
            If cp.IsAttached Then
                ms = FlowSheet.SimulationObjects(cp.AttachedConnector.AttachedTo.Name)
                With ms
                    .Clear()
                    .ClearAllProps()
                    .SpecType = Interfaces.Enums.StreamSpec.Pressure_and_Enthalpy
                    .SetTemperature(T)
                    .SetPressure(P)
                    .SetMassEnthalpy(MixedStream.Phases(2).Properties.enthalpy.GetValueOrDefault)
                    .SetMassFlow(MixedStream.Phases(2).Properties.massflow.GetValueOrDefault)
                    Dim comp As BaseClasses.Compound
                    For Each comp In .Phases(0).Compounds.Values
                        comp.MoleFraction = MixedStream.Phases(2).Compounds(comp.Name).MoleFraction.GetValueOrDefault
                        comp.MassFraction = MixedStream.Phases(2).Compounds(comp.Name).MassFraction.GetValueOrDefault
                    Next
                    .CopyCompositions(PhaseLabel.Mixture, PhaseLabel.Vapor)
                    .Phases(2).Properties.molarfraction = 1.0
                    .AtEquilibrium = True
                End With
            End If

            ' Calculate liquid densities
            PropertyPackage.CurrentMaterialStream = MixedStream
            Dim dens1 = DirectCast(PropertyPackage, PropertyPackages.PropertyPackage).AUX_LIQDENS(T, VnL1, P)
            Dim dens2 As Double = dens1
            If VnL2.Sum > 0 Then dens2 = DirectCast(PropertyPackage, PropertyPackages.PropertyPackage).AUX_LIQDENS(T, VnL2, P)
            If Double.IsNaN(dens1) Then dens1 = 0.0
            If Double.IsNaN(dens2) Then dens2 = 0.0

            If dens1 <= dens2 Then

                cp = Me.GraphicObject.OutputConnectors(1) 'liquid 1
                If cp.IsAttached Then
                    ms = FlowSheet.SimulationObjects(cp.AttachedConnector.AttachedTo.Name)
                    With ms
                        .Clear()
                        .ClearAllProps()
                        .SpecType = Interfaces.Enums.StreamSpec.Pressure_and_Enthalpy
                        .SetTemperature(T)
                        .SetPressure(P)
                        If W1 > 0.0# Then .SetMassFlow(W1) Else .SetMassFlow(0.0)
                        .SetMassEnthalpy(HL1)
                        Dim comp As BaseClasses.Compound
                        i = 0
                        For Each comp In .Phases(0).Compounds.Values
                            If W1 > 0 Then
                                comp.MoleFraction = VnL1(Vids.IndexOf(comp.Name))
                                comp.MassFraction = VmL1(Vids.IndexOf(comp.Name))
                            Else
                                comp.MoleFraction = MixedStream.Phases(3).Compounds(comp.Name).MoleFraction.GetValueOrDefault
                                comp.MassFraction = MixedStream.Phases(3).Compounds(comp.Name).MassFraction.GetValueOrDefault
                            End If
                            i += 1
                        Next
                        If WS = 0.0 Then
                            .CopyCompositions(PhaseLabel.Mixture, PhaseLabel.Liquid1)
                            .Phases(3).Properties.molarfraction = 1.0
                            .AtEquilibrium = True
                        End If
                    End With
                End If

                cp = Me.GraphicObject.OutputConnectors(2) 'liquid 2
                If cp.IsAttached Then
                    ms = FlowSheet.SimulationObjects(cp.AttachedConnector.AttachedTo.Name)
                    With ms
                        .Clear()
                        .ClearAllProps()
                        .SpecType = Interfaces.Enums.StreamSpec.Pressure_and_Enthalpy
                        .SetTemperature(T)
                        .SetPressure(P)
                        If W2 > 0.0# Then .SetMassFlow(W2) Else .SetMassFlow(0.0)
                        .SetMassEnthalpy(HL2)
                        Dim comp As BaseClasses.Compound
                        i = 0
                        For Each comp In .Phases(0).Compounds.Values
                            If W2 > 0 Then
                                comp.MoleFraction = VnL2(Vids.IndexOf(comp.Name))
                                comp.MassFraction = VmL2(Vids.IndexOf(comp.Name))
                            Else
                                comp.MoleFraction = MixedStream.Phases(4).Compounds(comp.Name).MoleFraction.GetValueOrDefault
                                comp.MassFraction = MixedStream.Phases(4).Compounds(comp.Name).MassFraction.GetValueOrDefault
                            End If
                            i += 1
                        Next
                        If WS = 0.0 Then
                            .CopyCompositions(PhaseLabel.Mixture, PhaseLabel.Liquid1)
                            .Phases(3).Properties.molarfraction = 1.0
                            .AtEquilibrium = True
                        End If
                    End With
                Else
                    If MixedStream.Phases(4).Properties.massflow.GetValueOrDefault > 0.0# Then Throw New Exception(FlowSheet.GetTranslatedString("SeparatorVessel_SecondLiquidPhaseFound"))
                End If

            Else

                cp = Me.GraphicObject.OutputConnectors(1) 'liquid 1
                If cp.IsAttached Then
                    ms = FlowSheet.SimulationObjects(cp.AttachedConnector.AttachedTo.Name)
                    With ms
                        .Clear()
                        .ClearAllProps()
                        .SpecType = Interfaces.Enums.StreamSpec.Pressure_and_Enthalpy
                        .Phases(0).Properties.temperature = T
                        .Phases(0).Properties.pressure = P
                        If W2 > 0.0# Then .Phases(0).Properties.massflow = W2 Else .Phases(0).Properties.molarflow = 0.0#
                        .Phases(0).Properties.enthalpy = HL2
                        Dim comp As BaseClasses.Compound
                        i = 0
                        For Each comp In .Phases(0).Compounds.Values
                            comp.MoleFraction = VnL2(Vids.IndexOf(comp.Name))
                            comp.MassFraction = VmL2(Vids.IndexOf(comp.Name))
                            i += 1
                        Next
                        If WS = 0.0 Then
                            .CopyCompositions(PhaseLabel.Mixture, PhaseLabel.Liquid1)
                            .Phases(3).Properties.molarfraction = 1.0
                            .AtEquilibrium = True
                        End If
                    End With
                End If

                cp = Me.GraphicObject.OutputConnectors(2) 'liquid 2
                If cp.IsAttached Then
                    ms = FlowSheet.SimulationObjects(cp.AttachedConnector.AttachedTo.Name)
                    With ms
                        .Clear()
                        .ClearAllProps()
                        .SpecType = Interfaces.Enums.StreamSpec.Pressure_and_Enthalpy
                        .Phases(0).Properties.temperature = T
                        .Phases(0).Properties.pressure = P
                        If W1 > 0.0# Then .Phases(0).Properties.massflow = W1 Else .Phases(0).Properties.molarflow = 0.0#
                        .Phases(0).Properties.enthalpy = HL1
                        Dim comp As BaseClasses.Compound
                        i = 0
                        For Each comp In .Phases(0).Compounds.Values
                            comp.MoleFraction = VnL1(Vids.IndexOf(comp.Name))
                            comp.MassFraction = VmL1(Vids.IndexOf(comp.Name))
                            i += 1
                        Next
                        If WS = 0.0 Then
                            .CopyCompositions(PhaseLabel.Mixture, PhaseLabel.Liquid1)
                            .Phases(3).Properties.molarfraction = 1.0
                            .AtEquilibrium = True
                        End If
                    End With
                Else
                    If MixedStream.Phases(3).Properties.massflow.GetValueOrDefault > 0.0# Then Throw New Exception(FlowSheet.GetTranslatedString("SeparatorVessel_SecondLiquidPhaseFound"))
                End If

            End If

            Hf = MixedStream.Phases(0).Properties.enthalpy.GetValueOrDefault * W
            Me.DeltaQ = Hf - H0

            ' Energy stream - update power value (kJ/s)
            If Me.GraphicObject.InputConnectors(6).IsAttached And CalculationMode = CalculationModes.Legacy Then
                With Me.GetInletEnergyStream(6)
                    .EnergyFlow = Me.DeltaQ.GetValueOrDefault
                    .GraphicObject.Calculated = True
                End With
            End If

            ' Store sizing data
            Me.rhol = MixedStream.Phases(1).Properties.density.GetValueOrDefault
            Me.rhov = MixedStream.Phases(2).Properties.density.GetValueOrDefault
            Me.ql = MixedStream.Phases(1).Properties.volumetric_flow.GetValueOrDefault
            Me.qv = MixedStream.Phases(2).Properties.volumetric_flow.GetValueOrDefault
            Me.wl = MixedStream.Phases(1).Properties.massflow.GetValueOrDefault
            Me.wv = MixedStream.Phases(2).Properties.massflow.GetValueOrDefault
            Me.rhoe = MixedStream.Phases(0).Properties.density.GetValueOrDefault
            Me.qe = MixedStream.Phases(0).Properties.volumetric_flow.GetValueOrDefault

            ' Perform Lazalde-Crabtree separation efficiency calculations
            CalculateLazaldeCrabtree()

        End Sub

        Public Overrides Sub DeCalculate()

            Dim j As Integer = 0
            Dim cp As IConnectionPoint

            cp = Me.GraphicObject.OutputConnectors(0)
            If cp.IsAttached Then
                With Me.GetOutletMaterialStream(0)
                    .Phases(0).Properties.temperature = Nothing
                    .Phases(0).Properties.pressure = Nothing
                    .Phases(0).Properties.enthalpy = Nothing
                    Dim comp As BaseClasses.Compound
                    j = 0
                    For Each comp In .Phases(0).Compounds.Values
                        comp.MoleFraction = 0
                        comp.MassFraction = 0
                        j += 1
                    Next
                    .Phases(0).Properties.massflow = Nothing
                    .Phases(0).Properties.massfraction = 1
                    .Phases(0).Properties.molarfraction = 1
                    .GraphicObject.Calculated = False
                End With
            End If

            cp = Me.GraphicObject.OutputConnectors(1)
            If cp.IsAttached Then
                With Me.GetOutletMaterialStream(1)
                    .Phases(0).Properties.temperature = Nothing
                    .Phases(0).Properties.pressure = Nothing
                    .Phases(0).Properties.enthalpy = Nothing
                    Dim comp As BaseClasses.Compound
                    j = 0
                    For Each comp In .Phases(0).Compounds.Values
                        comp.MoleFraction = 0
                        comp.MassFraction = 0
                        j += 1
                    Next
                    .Phases(0).Properties.massflow = Nothing
                    .Phases(0).Properties.massfraction = 1
                    .Phases(0).Properties.molarfraction = 1
                    .GraphicObject.Calculated = False
                End With
            End If

        End Sub

        Public Overrides Function GetPropertyValue(ByVal prop As String, Optional ByVal su As Interfaces.IUnitsOfMeasure = Nothing) As Object

            Dim val0 As Object = MyBase.GetPropertyValue(prop, su)

            If Not val0 Is Nothing Then
                Return val0
            Else
                If su Is Nothing Then su = New SystemsOfUnits.SI
                Dim value As Double = 0
                Dim propidx As Integer = Convert.ToInt32(prop.Split("_")(2))

                Select Case propidx
                    Case 0
                        value = SystemsOfUnits.Converter.ConvertFromSI(su.temperature, Me.FlashTemperature)
                    Case 1
                        value = SystemsOfUnits.Converter.ConvertFromSI(su.pressure, Me.FlashPressure)
                End Select

                Return value
            End If

        End Function

        Public Overloads Overrides Function GetProperties(ByVal proptype As Interfaces.Enums.PropertyType) As String()
            Dim i As Integer = 0
            Dim proplist As New ArrayList
            Dim basecol = MyBase.GetProperties(proptype)
            If basecol.Length > 0 Then proplist.AddRange(basecol)
            Select Case proptype
                Case PropertyType.RW
                    For i = 0 To 1
                        proplist.Add("PROP_SV_" + CStr(i))
                    Next
                Case PropertyType.WR
                    For i = 0 To 1
                        proplist.Add("PROP_SV_" + CStr(i))
                    Next
                Case PropertyType.ALL
                    For i = 0 To 1
                        proplist.Add("PROP_SV_" + CStr(i))
                    Next
            End Select
            Return proplist.ToArray(GetType(System.String))
        End Function

        Public Overrides Function SetPropertyValue(ByVal prop As String, ByVal propval As Object, Optional ByVal su As Interfaces.IUnitsOfMeasure = Nothing) As Boolean

            If MyBase.SetPropertyValue(prop, propval, su) Then Return True

            If su Is Nothing Then su = New SystemsOfUnits.SI
            Dim propidx As Integer = Convert.ToInt32(prop.Split("_")(2))

            Select Case propidx
                Case 0
                    Me.FlashTemperature = SystemsOfUnits.Converter.ConvertToSI(su.temperature, propval)
                Case 1
                    Me.FlashPressure = SystemsOfUnits.Converter.ConvertToSI(su.pressure, propval)
            End Select
            Return True
        End Function

        Public Overrides Function GetPropertyUnit(ByVal prop As String, Optional ByVal su As Interfaces.IUnitsOfMeasure = Nothing) As String

            Dim u0 As String = MyBase.GetPropertyUnit(prop, su)

            If u0 = "NF" Then
                If su Is Nothing Then su = New SystemsOfUnits.SI
                Dim value As String = ""
                Dim propidx As Integer = Convert.ToInt32(prop.Split("_")(2))

                Select Case propidx
                    Case 0
                        value = su.temperature
                    Case 1
                        value = su.pressure
                End Select

                Return value
            Else
                Return u0
            End If

        End Function

        Public Overrides Sub DisplayEditForm()

            If f Is Nothing Then
                f = New EditingForm_GeothermalSeparator With {.SeparatorObject = Me}
                f.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft
                f.Tag = "ObjectEditor"
                Me.FlowSheet.DisplayForm(f)
            Else
                If f.IsDisposed Then
                    f = New EditingForm_GeothermalSeparator With {.SeparatorObject = Me}
                    f.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft
                    f.Tag = "ObjectEditor"
                    Me.FlowSheet.DisplayForm(f)
                Else
                    f.Activate()
                End If
            End If

        End Sub

        Public Overrides Sub UpdateEditForm()
            If f IsNot Nothing Then
                If Not f.IsDisposed Then
                    ' Thread-safe UI update using Invoke
                    If f.InvokeRequired Then
                        f.BeginInvoke(Sub() f.UpdateInfo())
                    Else
                        f.UpdateInfo()
                    End If
                End If
            End If
        End Sub

        Public Overrides Function GetIconBitmap() As Object
            Dim assembly = System.Reflection.Assembly.GetExecutingAssembly()
            Using stream = assembly.GetManifestResourceStream("DWSIM.UnitOperations.separator.png")
                If stream IsNot Nothing Then
                    Return New System.Drawing.Bitmap(stream)
                End If
            End Using
            Return Nothing
        End Function

        Public Overrides Function GetDisplayDescription() As String
            Return "Geothermal Separator (Lazalde-Crabtree)"
        End Function

        Public Overrides Function GetDisplayName() As String
            Return "Geothermal Separator"
        End Function

        Public Overrides Sub CloseEditForm()
            If f IsNot Nothing Then
                If Not f.IsDisposed Then
                    f.Close()
                    f = Nothing
                End If
            End If
        End Sub

        Public Overrides ReadOnly Property MobileCompatible As Boolean
            Get
                Return True
            End Get
        End Property

#Region "IExternalUnitOperation Implementation"

        Private IconImage As SkiaSharp.SKImage

        Public Function ReturnInstance(typename As String) As Object Implements IExternalUnitOperation.ReturnInstance
            Return New GeothermalSeparator()
        End Function

        Public ReadOnly Property Prefix As String Implements IExternalUnitOperation.Prefix
            Get
                Return "GSEP"
            End Get
        End Property

        Public ReadOnly Property ExternalName As String Implements IExternalUnitOperation.Name
            Get
                Return "Geothermal Separator"
            End Get
        End Property

        Public ReadOnly Property ExternalDescription As String Implements IExternalUnitOperation.Description
            Get
                Return "Geothermal Separator (Lazalde-Crabtree Method)"
            End Get
        End Property

        Public Sub Draw(g As Object) Implements IExternalUnitOperation.Draw
            Dim canvas = DirectCast(g, SkiaSharp.SKCanvas)

            Dim X = GraphicObject.X
            Dim Y = GraphicObject.Y
            Dim Width = GraphicObject.Width
            Dim Height = GraphicObject.Height

            ' Check DrawMode from GraphicObject
            Dim drawMode As Integer = 0
            Try
                drawMode = CInt(GraphicObject.GetType().GetProperty("DrawMode")?.GetValue(GraphicObject))
            Catch
                drawMode = 0
            End Try

            Select Case drawMode
                Case 2
                    ' Icon mode - draw PNG image
                    If IconImage Is Nothing Then
                        Dim iconBitmap = DirectCast(GetIconBitmap(), System.Drawing.Bitmap)
                        If iconBitmap IsNot Nothing Then
                            Using skBitmap = iconBitmap.ToSKBitmap()
                                IconImage = SkiaSharp.SKImage.FromBitmap(skBitmap)
                            End Using
                        End If
                    End If

                    If IconImage IsNot Nothing Then
                        Using p As New SkiaSharp.SKPaint With {.FilterQuality = SkiaSharp.SKFilterQuality.High}
                            canvas.DrawImage(IconImage, New SkiaSharp.SKRect(X, Y, X + Width, Y + Height), p)
                        End Using
                    End If

                Case Else
                    ' Default (0) or B/W (1) mode - draw vector graphics
                    Dim lineColor As SkiaSharp.SKColor = SkiaSharp.SKColors.SteelBlue
                    If drawMode = 1 Then
                        lineColor = SkiaSharp.SKColors.Black
                    Else
                        ' Get line color from status
                        Try
                            Dim status = GraphicObject.Status
                            Select Case status
                                Case Interfaces.Enums.GraphicObjects.Status.Calculated
                                    lineColor = SkiaSharp.SKColors.SteelBlue
                                Case Interfaces.Enums.GraphicObjects.Status.Calculating
                                    lineColor = SkiaSharp.SKColors.YellowGreen
                                Case Interfaces.Enums.GraphicObjects.Status.ErrorCalculating
                                    lineColor = SkiaSharp.SKColors.Salmon
                                Case Interfaces.Enums.GraphicObjects.Status.Idle
                                    lineColor = SkiaSharp.SKColors.SteelBlue
                                Case Interfaces.Enums.GraphicObjects.Status.Inactive
                                    lineColor = SkiaSharp.SKColors.Gray
                                Case Interfaces.Enums.GraphicObjects.Status.NotCalculated
                                    lineColor = SkiaSharp.SKColors.Salmon
                                Case Interfaces.Enums.GraphicObjects.Status.Modified
                                    lineColor = SkiaSharp.SKColors.LightGreen
                            End Select
                        Catch
                        End Try
                    End If

                    ' Outline pen
                    Using myPen As New SkiaSharp.SKPaint()
                        myPen.Color = lineColor
                        myPen.StrokeWidth = 1
                        myPen.IsStroke = True
                        myPen.IsAntialias = True

                        ' Fill pen (semi-transparent)
                        Using gradPen As New SkiaSharp.SKPaint()
                            If drawMode = 0 Then
                                gradPen.Color = lineColor.WithAlpha(50)
                            Else
                                gradPen.Color = SkiaSharp.SKColors.Transparent
                            End If
                            gradPen.IsStroke = False
                            gradPen.IsAntialias = True

                            ' Vessel body dimensions (same width as original, 50% taller)
                            Dim vesselLeft = X + 0.25 * Width
                            Dim vesselRight = X + 0.75 * Width
                            Dim vesselWidth = vesselRight - vesselLeft

                            ' Main body rectangle (taller proportions - 50% longer than original)
                            Dim bodyRect As New SkiaSharp.SKRect(vesselLeft, Y + 0.1 * Height, vesselRight, Y + 0.9 * Height)
                            canvas.DrawRect(bodyRect, gradPen)
                            canvas.DrawRect(bodyRect, myPen)

                            ' Top dome (elliptical arc)
                            Dim topArcRect As New SkiaSharp.SKRect(vesselLeft, Y, vesselRight, Y + 0.2 * Height)
                            canvas.DrawArc(topArcRect, -180, 180, False, gradPen)
                            canvas.DrawArc(topArcRect, -180, 180, False, myPen)

                            ' Bottom dome (elliptical arc)
                            Dim bottomArcRect As New SkiaSharp.SKRect(vesselLeft, Y + 0.8 * Height, vesselRight, Y + Height)
                            canvas.DrawArc(bottomArcRect, 0, 180, False, gradPen)
                            canvas.DrawArc(bottomArcRect, 0, 180, False, myPen)

                            ' === Geothermal Separator Internals ===

                            ' Steam tube parameters
                            Dim tubeWidth = 0.15 * vesselWidth
                            Dim tubeCenterX = vesselLeft + vesselWidth / 2
                            Dim tubeLeft = tubeCenterX - tubeWidth / 2
                            Dim tubeRight = tubeCenterX + tubeWidth / 2
                            Dim tubeTop = Y + 0.06 * Height
                            Dim bendStartY = Y + 0.58 * Height  ' Where vertical section ends
                            Dim bendRadius = tubeWidth          ' Bend radius

                            ' Steam tube - straight vertical section (from top to bend)
                            canvas.DrawLine(tubeLeft, tubeTop, tubeLeft, bendStartY, myPen)
                            canvas.DrawLine(tubeRight, tubeTop, tubeRight, bendStartY, myPen)

                            ' Steam tube - 90° bend turning RIGHT
                            ' Outer curve (left edge curves down to horizontal) - center at inner corner (tubeRight, bendStartY)
                            ' In SkiaSharp: 180° = left, 90° = down. Sweep -90° goes from left to down (clockwise visually)
                            Dim outerBendRect As New SkiaSharp.SKRect(tubeRight - tubeWidth, bendStartY - tubeWidth, tubeRight + tubeWidth, bendStartY + tubeWidth)
                            canvas.DrawArc(outerBendRect, 180, -90, False, myPen)

                            ' Inner curve (right edge) - sharp corner at (tubeRight, bendStartY)
                            ' Just connect with lines for clean look

                            ' Steam tube - horizontal section to vessel wall
                            Dim horizontalY = bendStartY + tubeWidth
                            canvas.DrawLine(tubeRight, bendStartY, vesselRight, bendStartY, myPen)
                            canvas.DrawLine(tubeRight, bendStartY + tubeWidth, vesselRight, bendStartY + tubeWidth, myPen)

                            ' Internal baffle plate (below steam tube)
                            Dim plateY = Y + 0.72 * Height
                            Dim plateGap = 0.08 * vesselWidth
                            canvas.DrawLine(vesselLeft + 0.05 * vesselWidth, plateY, tubeCenterX - plateGap, plateY, myPen)
                            canvas.DrawLine(tubeCenterX + plateGap, plateY, vesselRight - 0.05 * vesselWidth, plateY, myPen)

                        End Using
                    End Using

            End Select
        End Sub

        Public Sub CreateConnectors() Implements IExternalUnitOperation.CreateConnectors
            Dim X = GraphicObject.X
            Dim Y = GraphicObject.Y
            Dim Width = GraphicObject.Width
            Dim Height = GraphicObject.Height

            ' Vessel body dimensions (must match Draw method - same width as original)
            Dim vesselLeft = X + 0.25 * Width
            Dim vesselRight = X + 0.75 * Width
            Dim vesselWidth = vesselRight - vesselLeft

            ' Steam tube parameters (must match Draw method)
            Dim tubeWidth = 0.15 * vesselWidth
            Dim bendStartY = Y + 0.58 * Height  ' Where vertical section ends

            ' Connector positions
            Dim inletY = Y + 0.4 * Height  ' Left side inlet
            Dim steamOutletY = bendStartY + tubeWidth / 2  ' Right side at steam tube center
            Dim liquidOutletX = X + 0.5 * Width  ' Bottom center

            ' Create 7 input connectors (6 material streams + 1 energy stream)
            ' All inlet connectors on left side
            For i As Integer = 0 To 5
                If GraphicObject.InputConnectors.Count <= i Then
                    Dim portIn As New Drawing.SkiaSharp.GraphicObjects.ConnectionPoint()
                    portIn.IsEnergyConnector = False
                    portIn.Type = Interfaces.Enums.GraphicObjects.ConType.ConIn
                    portIn.Position = New DWSIM.DrawingTools.Point.Point(vesselLeft, inletY)
                    portIn.ConnectorName = "Inlet " & (i + 1).ToString()
                    portIn.Direction = Interfaces.Enums.GraphicObjects.ConDir.Right
                    GraphicObject.InputConnectors.Add(portIn)
                End If
            Next

            ' Input 6: Energy stream (bottom left)
            If GraphicObject.InputConnectors.Count <= 6 Then
                Dim portEnergy As New Drawing.SkiaSharp.GraphicObjects.ConnectionPoint()
                portEnergy.IsEnergyConnector = True
                portEnergy.Type = Interfaces.Enums.GraphicObjects.ConType.ConEn
                portEnergy.Position = New DWSIM.DrawingTools.Point.Point(vesselLeft, Y + Height)
                portEnergy.ConnectorName = "Energy"
                portEnergy.Direction = Interfaces.Enums.GraphicObjects.ConDir.Up
                GraphicObject.InputConnectors.Add(portEnergy)
            End If

            ' Create 4 output connectors (only 2 used for geothermal separator)
            ' Output 0: Steam outlet (right side at steam tube exit)
            If GraphicObject.OutputConnectors.Count < 1 Then
                Dim portSteam As New Drawing.SkiaSharp.GraphicObjects.ConnectionPoint()
                portSteam.IsEnergyConnector = False
                portSteam.Type = Interfaces.Enums.GraphicObjects.ConType.ConOut
                portSteam.Position = New DWSIM.DrawingTools.Point.Point(vesselRight, steamOutletY)
                portSteam.ConnectorName = "Steam Out"
                portSteam.Direction = Interfaces.Enums.GraphicObjects.ConDir.Right
                GraphicObject.OutputConnectors.Add(portSteam)
            End If

            ' Output 1: Liquid outlet (bottom center)
            If GraphicObject.OutputConnectors.Count < 2 Then
                Dim portLiquid As New Drawing.SkiaSharp.GraphicObjects.ConnectionPoint()
                portLiquid.IsEnergyConnector = False
                portLiquid.Type = Interfaces.Enums.GraphicObjects.ConType.ConOut
                portLiquid.Position = New DWSIM.DrawingTools.Point.Point(liquidOutletX, Y + Height)
                portLiquid.ConnectorName = "Brine Out"
                portLiquid.Direction = Interfaces.Enums.GraphicObjects.ConDir.Down
                GraphicObject.OutputConnectors.Add(portLiquid)
            End If

            ' Output 2: Not used but required for Vessel compatibility
            If GraphicObject.OutputConnectors.Count < 3 Then
                Dim portLiquid2 As New Drawing.SkiaSharp.GraphicObjects.ConnectionPoint()
                portLiquid2.IsEnergyConnector = False
                portLiquid2.Type = Interfaces.Enums.GraphicObjects.ConType.ConOut
                portLiquid2.Position = New DWSIM.DrawingTools.Point.Point(liquidOutletX, Y + Height)
                portLiquid2.ConnectorName = "Liquid 2 Out"
                portLiquid2.Direction = Interfaces.Enums.GraphicObjects.ConDir.Down
                GraphicObject.OutputConnectors.Add(portLiquid2)
            End If

            ' Output 3: Not used but required for Vessel compatibility
            If GraphicObject.OutputConnectors.Count < 4 Then
                Dim portRecirc As New Drawing.SkiaSharp.GraphicObjects.ConnectionPoint()
                portRecirc.IsEnergyConnector = False
                portRecirc.Type = Interfaces.Enums.GraphicObjects.ConType.ConOut
                portRecirc.Position = New DWSIM.DrawingTools.Point.Point(liquidOutletX, Y + Height)
                portRecirc.ConnectorName = "Recirculation"
                portRecirc.Direction = Interfaces.Enums.GraphicObjects.ConDir.Down
                GraphicObject.OutputConnectors.Add(portRecirc)
            End If

            ' Update connector positions (always update to handle moves/resizes)
            ' Recalculate positions based on current graphic object location
            vesselLeft = GraphicObject.X + 0.25 * GraphicObject.Width
            vesselRight = GraphicObject.X + 0.75 * GraphicObject.Width
            vesselWidth = vesselRight - vesselLeft
            tubeWidth = 0.15 * vesselWidth
            bendStartY = GraphicObject.Y + 0.58 * GraphicObject.Height
            inletY = GraphicObject.Y + 0.4 * GraphicObject.Height
            steamOutletY = bendStartY + tubeWidth / 2
            liquidOutletX = GraphicObject.X + 0.5 * GraphicObject.Width

            ' Update input connector positions (all at vessel left wall)
            For i As Integer = 0 To 5
                If GraphicObject.InputConnectors.Count > i Then
                    GraphicObject.InputConnectors(i).Position = New DWSIM.DrawingTools.Point.Point(vesselLeft, inletY)
                    GraphicObject.InputConnectors(i).Direction = Interfaces.Enums.GraphicObjects.ConDir.Right
                End If
            Next

            ' Energy connector (at bottom)
            If GraphicObject.InputConnectors.Count > 6 Then
                GraphicObject.InputConnectors(6).Position = New DWSIM.DrawingTools.Point.Point(vesselLeft, GraphicObject.Y + GraphicObject.Height)
                GraphicObject.InputConnectors(6).Direction = Interfaces.Enums.GraphicObjects.ConDir.Up
            End If

            ' Output connectors
            ' Steam outlet (right side at steam tube exit)
            If GraphicObject.OutputConnectors.Count > 0 Then
                GraphicObject.OutputConnectors(0).Position = New DWSIM.DrawingTools.Point.Point(vesselRight, steamOutletY)
                GraphicObject.OutputConnectors(0).Direction = Interfaces.Enums.GraphicObjects.ConDir.Right
            End If
            ' Brine/Liquid outlet (bottom center)
            If GraphicObject.OutputConnectors.Count > 1 Then
                GraphicObject.OutputConnectors(1).Position = New DWSIM.DrawingTools.Point.Point(liquidOutletX, GraphicObject.Y + GraphicObject.Height)
                GraphicObject.OutputConnectors(1).Direction = Interfaces.Enums.GraphicObjects.ConDir.Down
            End If
            ' Hidden connectors for compatibility (positioned at bottom)
            If GraphicObject.OutputConnectors.Count > 2 Then
                GraphicObject.OutputConnectors(2).Position = New DWSIM.DrawingTools.Point.Point(liquidOutletX, GraphicObject.Y + GraphicObject.Height)
                GraphicObject.OutputConnectors(2).Direction = Interfaces.Enums.GraphicObjects.ConDir.Down
            End If
            If GraphicObject.OutputConnectors.Count > 3 Then
                GraphicObject.OutputConnectors(3).Position = New DWSIM.DrawingTools.Point.Point(liquidOutletX, GraphicObject.Y + GraphicObject.Height)
                GraphicObject.OutputConnectors(3).Direction = Interfaces.Enums.GraphicObjects.ConDir.Down
            End If
        End Sub

        Public Sub PopulateEditorPanel(container As Object) Implements IExternalUnitOperation.PopulateEditorPanel
            ' Use default property grid editor
        End Sub

#End Region

#Region "Lazalde-Crabtree Calculation Methods"

        ''' <summary>
        ''' Reference constants for Baker flow pattern map
        ''' </summary>
        Private Const RHO_AIR As Double = 1.23        ' kg/m³ at 20°C, 1 atm
        Private Const RHO_WATER As Double = 1000.0    ' kg/m³
        Private Const SIGMA_WATER As Double = 0.0728  ' N/m (surface tension)
        Private Const MU_WATER As Double = 0.001      ' Pa·s (viscosity)

        ''' <summary>
        ''' Detects the two-phase flow pattern using Baker's method
        ''' </summary>
        ''' <param name="W_V">Steam mass flow [kg/s]</param>
        ''' <param name="W_L">Water mass flow [kg/s]</param>
        ''' <param name="rho_V">Steam density [kg/m³]</param>
        ''' <param name="rho_L">Water density [kg/m³]</param>
        ''' <param name="sigma_L">Water surface tension [N/m]</param>
        ''' <param name="mu_L">Water viscosity [Pa·s]</param>
        ''' <param name="A_pipe">Pipe cross-sectional area [m²]</param>
        ''' <returns>Detected flow pattern</returns>
        Private Function DetectBakerFlowPattern(W_V As Double, W_L As Double,
                                                 rho_V As Double, rho_L As Double,
                                                 sigma_L As Double, mu_L As Double,
                                                 A_pipe As Double) As FlowPatterns
            ' Calculate mass fluxes [kg/m²·s]
            Dim G_G As Double = W_V / A_pipe  ' Gas mass flux
            Dim G_L As Double = W_L / A_pipe  ' Liquid mass flux

            ' Calculate Baker parameters
            Dim lambda As Double = Math.Sqrt((rho_V / RHO_AIR) * (rho_L / RHO_WATER))
            Dim psi As Double = (SIGMA_WATER / sigma_L) * Math.Pow((mu_L / MU_WATER) * Math.Pow(RHO_WATER / rho_L, 2), 1.0 / 3.0)

            ' Baker chart coordinates
            Dim Bx As Double = (G_L / G_G) * (rho_V / RHO_AIR) * Math.Sqrt(rho_L / RHO_WATER)
            Dim By As Double = G_G / (lambda * psi)

            ' Prevent division issues
            If Bx <= 0 OrElse Double.IsNaN(Bx) OrElse Double.IsInfinity(Bx) Then
                Return FlowPatterns.Annular
            End If
            If By <= 0 OrElse Double.IsNaN(By) OrElse Double.IsInfinity(By) Then
                Return FlowPatterns.Stratified
            End If

            ' Flow regime boundaries (from Baker chart)
            ' Dispersed/Bubble: By > 5.1 × Bx^0.5 for Bx > 4000
            If Bx > 4000 AndAlso By > 5.1 * Math.Sqrt(Bx) Then
                Return FlowPatterns.Dispersed
            End If

            ' Annular: By > 2040 / Bx^0.8 for Bx < 0.5
            If Bx < 0.5 AndAlso By > 2040 / Math.Pow(Bx, 0.8) Then
                Return FlowPatterns.Annular
            End If

            ' Stratified: By < 10 for Bx > 10
            If Bx > 10 AndAlso By < 10 Then
                Return FlowPatterns.Stratified
            End If

            ' Slug/Plug: By < 800 / Bx for Bx > 1
            If Bx > 1 AndAlso By < 800 / Bx Then
                Return FlowPatterns.PlugSlug
            End If

            ' Default to annular for geothermal conditions (high vapor fraction)
            Return FlowPatterns.Annular
        End Function

        ''' <summary>
        ''' Gets Baker chart coordinates for the current operating point
        ''' </summary>
        Public Function GetBakerCoordinates(W_V As Double, W_L As Double,
                                            rho_V As Double, rho_L As Double,
                                            sigma_L As Double, mu_L As Double,
                                            A_pipe As Double) As (Bx As Double, By As Double)
            ' Calculate mass fluxes [kg/m²·s]
            Dim G_G As Double = W_V / A_pipe
            Dim G_L As Double = W_L / A_pipe

            ' Calculate Baker parameters
            Dim lambda As Double = Math.Sqrt((rho_V / RHO_AIR) * (rho_L / RHO_WATER))
            Dim psi As Double = (SIGMA_WATER / sigma_L) * Math.Pow((mu_L / MU_WATER) * Math.Pow(RHO_WATER / rho_L, 2), 1.0 / 3.0)

            ' Baker chart coordinates
            Dim Bx As Double = (G_L / G_G) * (rho_V / RHO_AIR) * Math.Sqrt(rho_L / RHO_WATER)
            Dim By As Double = G_G / (lambda * psi)

            Return (Bx, By)
        End Function

        ''' <summary>
        ''' Calculates drop diameter using modified Nukiyama-Tanasawa equation
        ''' </summary>
        ''' <param name="V_T">Inlet steam velocity [m/s]</param>
        ''' <param name="Q_VS">Steam volumetric flow [m³/s]</param>
        ''' <param name="Q_L">Water volumetric flow [m³/s]</param>
        ''' <param name="rho_L">Water density [kg/m³]</param>
        ''' <param name="sigma_L">Surface tension [N/m]</param>
        ''' <param name="mu_L">Water viscosity [Pa·s]</param>
        ''' <param name="flowPattern">Flow pattern</param>
        ''' <param name="X_s">Inlet steam quality</param>
        ''' <returns>Drop diameter [microns]</returns>
        Private Function CalculateDropDiameter(V_T As Double, Q_VS As Double, Q_L As Double,
                                                rho_L As Double, sigma_L As Double, mu_L As Double,
                                                flowPattern As FlowPatterns, X_s As Double) As Double
            ' Constants from Lazalde-Crabtree
            Const A As Double = 66.2898
            Const K As Double = 1357.35
            Const b As Double = 0.225
            Const c As Double = 0.5507

            ' Flow-pattern dependent constants
            Dim a_const As Double
            Dim B_const As Double
            Dim e_const As Double

            Select Case flowPattern
                Case FlowPatterns.Stratified
                    a_const = 0.5436
                    B_const = 94.9042 * X_s
                    e_const = -0.4538
                Case FlowPatterns.Annular
                    a_const = 0.8069
                    B_const = 198.7749 * X_s
                    e_const = 0.2628
                Case FlowPatterns.Dispersed
                    a_const = 0.8069
                    B_const = 140.8346 * X_s
                    e_const = 0.5747
                Case FlowPatterns.PlugSlug
                    a_const = 0.5436
                    B_const = 37.3618 * X_s
                    e_const = -0.0000688
                Case Else
                    ' Default to annular
                    a_const = 0.8069
                    B_const = 198.7749 * X_s
                    e_const = 0.2628
            End Select

            ' Convert units for formula:
            ' ρ_L in g/cm³, σ_L in dyne/cm, μ_L in poise
            Dim rho_L_cgs As Double = rho_L / 1000.0        ' kg/m³ → g/cm³
            Dim sigma_L_cgs As Double = sigma_L * 1000.0    ' N/m → dyne/cm
            Dim mu_L_cgs As Double = mu_L * 10.0            ' Pa·s → poise

            ' Prevent division by zero
            If V_T < 0.1 Then V_T = 0.1
            If Q_VS < 0.0001 Then Q_VS = 0.0001
            If rho_L_cgs < 0.001 Then rho_L_cgs = 0.001
            If sigma_L_cgs < 0.001 Then sigma_L_cgs = 0.001
            If mu_L_cgs < 0.00001 Then mu_L_cgs = 0.00001

            ' Nukiyama-Tanasawa equation (output in microns)
            Dim term1 As Double = (A / Math.Pow(V_T, a_const)) * Math.Sqrt(sigma_L_cgs / rho_L_cgs)
            Dim term2 As Double = B_const * K * Math.Pow(mu_L_cgs * mu_L_cgs / (sigma_L_cgs * rho_L_cgs), b)
            Dim term3 As Double = Math.Pow(Q_L / Q_VS, c) * Math.Pow(V_T, e_const)

            Dim d_w As Double = term1 + term2 * term3

            ' Ensure reasonable bounds (1-1000 microns)
            If d_w < 1 Then d_w = 1
            If d_w > 1000 Then d_w = 1000

            Return d_w
        End Function

        ''' <summary>
        ''' Calculates centrifugal separation efficiency (eta_m)
        ''' </summary>
        ''' <param name="D">Vessel diameter [m]</param>
        ''' <param name="D_e">Steam outlet diameter [m]</param>
        ''' <param name="A_o">Inlet area [m²]</param>
        ''' <param name="u">Tangential velocity [m/s]</param>
        ''' <param name="d_w">Drop diameter [m]</param>
        ''' <param name="rho_L">Water density [kg/m³]</param>
        ''' <param name="mu_V">Steam viscosity [Pa·s]</param>
        ''' <param name="T">Temperature [°C]</param>
        ''' <param name="Q_VS">Steam volumetric flow [m³/s]</param>
        ''' <param name="Z">Total height [m]</param>
        ''' <param name="alpha">Lip position [m]</param>
        ''' <returns>Centrifugal efficiency (0-1)</returns>
        Private Function CalculateCentrifugalEfficiency(D As Double, D_e As Double, A_o As Double,
                                                         u As Double, d_w As Double, rho_L As Double,
                                                         mu_V As Double, T As Double, Q_VS As Double,
                                                         Z As Double, alpha As Double) As Double
            ' Validate inputs
            If D <= 0 OrElse D_e <= 0 OrElse A_o <= 0 OrElse u <= 0 OrElse d_w <= 0 Then
                Return 0.0
            End If

            ' Convert drop diameter from microns to meters
            Dim d_w_m As Double = d_w * 0.000001

            ' Calculate exponent n
            Dim n1 As Double = 0.6689 * Math.Pow(D, 0.14)
            Dim T_K As Double = T + 273.15
            Dim n As Double = 1 - (1 - n1) * Math.Pow(294.3 / T_K, 0.3)

            ' Calculate separator volumes
            ' VO_S = (π/4) × (D² - A_o) × Z
            Dim VO_S As Double = (Math.PI / 4) * (D * D - A_o) * Z

            ' Head volumes (ASME flanged & dished)
            ' VO₁ = (π×D²/4) × α
            Dim VO_1 As Double = (Math.PI * D * D / 4) * Math.Abs(alpha)
            ' VO₂ = 0.081 × D³
            Dim VO_2 As Double = 0.081 * D * D * D
            ' VO₃ = (π×D_e²/4) × (α + 0.169×D)
            Dim VO_3 As Double = (Math.PI * D_e * D_e / 4) * (Math.Abs(alpha) + 0.169 * D)

            Dim VO_H As Double = VO_1 + VO_2 - VO_3

            ' Residence times
            If Q_VS < 0.0001 Then Q_VS = 0.0001
            Dim t_mi As Double = VO_S / Q_VS   ' Minimum residence time [s]
            Dim t_ma As Double = VO_H / Q_VS   ' Additional time in head [s]
            Dim t_r As Double = t_mi + t_ma / 2.0  ' Total residence time [s]

            ' K_c parameter
            Dim K_c As Double = t_r * Q_VS / (D * D * D)

            ' C parameter
            Dim C As Double = 8 * K_c * D * D / A_o

            ' Psi parameter
            If mu_V < 0.000001 Then mu_V = 0.000001
            Dim psi As Double = (rho_L * d_w_m * d_w_m * (n + 1) * u) / (18 * mu_V * D)

            ' Centrifugal efficiency
            Dim exponent As Double = 2 * Math.Pow(psi * C, 1.0 / (2 * n + 2))
            Dim eta_m As Double = 1 - Math.Exp(-exponent)

            ' Clamp to valid range
            If eta_m < 0 Then eta_m = 0
            If eta_m > 1 Then eta_m = 1

            Return eta_m
        End Function

        ''' <summary>
        ''' Calculates entrainment efficiency (eta_A) based on annular velocity
        ''' </summary>
        ''' <param name="V_AN">Annular upward velocity [m/s]</param>
        ''' <returns>Entrainment efficiency (0-1)</returns>
        Private Function CalculateEntrainmentEfficiency(V_AN As Double) As Double
            ' From Lazalde-Crabtree:
            ' η_A = 10^j
            ' j = -3.384 × 10^(-14) × V_AN^13.9241

            If V_AN <= 0 Then Return 1.0

            Dim j As Double = -3.384E-14 * Math.Pow(V_AN, 13.9241)

            ' Limit j to prevent numerical issues
            If j < -10 Then j = -10  ' η_A = 1E-10 minimum
            If j > 0 Then j = 0      ' η_A = 1 maximum

            Dim eta_A As Double = Math.Pow(10, j)

            ' Clamp to valid range
            If eta_A < 0 Then eta_A = 0
            If eta_A > 1 Then eta_A = 1

            Return eta_A
        End Function

        ''' <summary>
        ''' Calculates outlet steam quality accounting for water carryover
        ''' </summary>
        ''' <param name="W_V">Steam mass flow [kg/s]</param>
        ''' <param name="W_L">Water mass flow [kg/s]</param>
        ''' <param name="eta_ef">Overall efficiency (0-1)</param>
        ''' <returns>Outlet steam quality (0-1)</returns>
        Private Function CalculateOutletQuality(W_V As Double, W_L As Double, eta_ef As Double) As Double
            ' Special cases
            If eta_ef >= 1.0 Then Return 1.0
            If eta_ef <= 0.0 Then Return W_V / (W_V + W_L)  ' Inlet quality
            If W_L <= 0 Then Return 1.0  ' Pure vapor

            ' X_o = (W_V/W_L) / (1 - η_ef + W_V/W_L)
            Dim ratio As Double = W_V / W_L
            Dim X_o As Double = ratio / (1 - eta_ef + ratio)

            ' Clamp to valid range
            If X_o < 0 Then X_o = 0
            If X_o > 1 Then X_o = 1

            Return X_o
        End Function

        ''' <summary>
        ''' Calculates separator pressure drop
        ''' </summary>
        ''' <param name="u">Tangential velocity [m/s]</param>
        ''' <param name="rho_V">Steam density [kg/m³]</param>
        ''' <param name="A_inlet">Inlet area [m²]</param>
        ''' <param name="D_e">Steam outlet diameter [m]</param>
        ''' <param name="equipType">Equipment type (Separator or Dryer)</param>
        ''' <param name="D_t">Inlet pipe diameter [m]</param>
        ''' <returns>Pressure drop [Pa]</returns>
        Private Function CalculatePressureDrop(u As Double, rho_V As Double, A_inlet As Double,
                                                D_e As Double, equipType As SeparatorType, D_t As Double) As Double
            ' ΔP = (N_H × u² × ρ_v) / 2 [Pa]
            ' N_H = 16 × (A_inlet) / D_e²

            If D_e <= 0 Then Return 0

            Dim N_H As Double
            If equipType = SeparatorType.Separator Then
                ' Spiral inlet: A_inlet ≈ A_e × B_e ≈ D_t²
                N_H = 16 * A_inlet / (D_e * D_e)
            Else
                ' Tangential inlet: A_inlet = π×D_T²/4
                N_H = 16 * (Math.PI * D_t * D_t / 4) / (D_e * D_e)
            End If

            ' Store for display
            Me.VelocityHeads = N_H

            Dim deltaP As Double = (N_H * u * u * rho_V) / 2.0

            Return deltaP
        End Function

        ''' <summary>
        ''' Calculates separator dimensions based on Lazalde-Crabtree ratios
        ''' </summary>
        Private Sub CalculateSeparatorDimensions()
            Dim D_t As Double = Me.InletPipeDiameter

            If EquipmentType = SeparatorType.Separator Then
                ' Separator (X_i < 95%)
                Me.VesselDiameter = 3.3 * D_t
                Me.SteamOutletDiameter = 1.0 * D_t
                Me.WaterOutletDiameter = 1.0 * D_t
                Me.LipPosition = -0.15 * D_t
                Me.CycloneHeight = 3.5 * D_t
                Me.TotalHeight = 5.5 * D_t
                ' Spiral inlet area ≈ D_t²
                Me.InletArea = D_t * D_t
            Else
                ' Dryer (X_i > 95%)
                Me.VesselDiameter = 3.5 * D_t
                Me.SteamOutletDiameter = 1.0 * D_t
                Me.WaterOutletDiameter = 0.0  ' Drain only
                Me.LipPosition = -0.15 * D_t
                Me.CycloneHeight = 3.0 * D_t
                Me.TotalHeight = 4.0 * D_t
                ' Tangential inlet area = π×D_t²/4
                Me.InletArea = Math.PI * D_t * D_t / 4
            End If
        End Sub

        ''' <summary>
        ''' Performs Lazalde-Crabtree separation efficiency calculations
        ''' Call this after the flash calculation in Calculate()
        ''' </summary>
        Private Sub CalculateLazaldeCrabtree()
            Try
                ' Get flash results from MixedStream
                Dim W_V As Double = MixedStream.Phases(2).Properties.massflow.GetValueOrDefault
                Dim W_L As Double = MixedStream.Phases(1).Properties.massflow.GetValueOrDefault +
                                    MixedStream.Phases(3).Properties.massflow.GetValueOrDefault +
                                    MixedStream.Phases(4).Properties.massflow.GetValueOrDefault
                Dim W_M As Double = W_V + W_L

                ' Get thermodynamic properties
                Dim rho_V As Double = MixedStream.Phases(2).Properties.density.GetValueOrDefault
                Dim rho_L As Double = MixedStream.Phases(1).Properties.density.GetValueOrDefault
                If rho_L <= 0 Then rho_L = MixedStream.Phases(3).Properties.density.GetValueOrDefault
                Dim mu_V As Double = MixedStream.Phases(2).Properties.viscosity.GetValueOrDefault
                Dim mu_L As Double = MixedStream.Phases(1).Properties.viscosity.GetValueOrDefault
                If mu_L <= 0 Then mu_L = MixedStream.Phases(3).Properties.viscosity.GetValueOrDefault
                Dim T As Double = MixedStream.Phases(0).Properties.temperature.GetValueOrDefault - 273.15  ' Convert K to °C
                Dim P As Double = MixedStream.Phases(0).Properties.pressure.GetValueOrDefault

                ' Surface tension - use approximate correlation if not available
                Dim sigma_L As Double = 0.0728  ' Default water at 20°C
                Try
                    ' Estimate surface tension from temperature (water)
                    ' σ ≈ 0.0728 × (1 - T/647.1)^1.2 for water
                    sigma_L = 0.0728 * Math.Pow(1 - (T + 273.15) / 647.1, 1.2)
                    If sigma_L < 0.001 Then sigma_L = 0.001
                Catch
                    sigma_L = 0.0728
                End Try

                ' Calculate inlet steam quality
                If W_M > 0 Then
                    Me.InletSteamQuality = W_V / W_M
                Else
                    Me.InletSteamQuality = 0
                End If

                ' Auto-detect equipment type based on inlet quality
                If Me.InletSteamQuality > 0.95 Then
                    Me.EquipmentType = SeparatorType.Dryer
                Else
                    Me.EquipmentType = SeparatorType.Separator
                End If

                ' Calculate volumetric flows
                Dim Q_VS As Double = 0
                Dim Q_L As Double = 0
                If rho_V > 0 Then Q_VS = W_V / rho_V
                If rho_L > 0 Then Q_L = W_L / rho_L

                ' Auto-size inlet pipe diameter if in AutoSize mode
                If SizingMode = SizingModes.AutoSize Then
                    ' V_T = 4 × Q_VS / (π × D_t²)
                    ' Solve for D_t: D_t = √(4 × Q_VS / (π × V_T))
                    If DesignSteamVelocity > 0 AndAlso Q_VS > 0 Then
                        Me.InletPipeDiameter = Math.Sqrt(4 * Q_VS / (Math.PI * DesignSteamVelocity))
                        ' Round to reasonable size (0.01m increments)
                        Me.InletPipeDiameter = Math.Round(Me.InletPipeDiameter, 2)
                        If Me.InletPipeDiameter < 0.1 Then Me.InletPipeDiameter = 0.1
                        If Me.InletPipeDiameter > 2.0 Then Me.InletPipeDiameter = 2.0
                    End If
                End If

                ' Calculate separator dimensions
                CalculateSeparatorDimensions()

                ' Calculate pipe area
                Dim A_pipe As Double = Math.PI * Me.InletPipeDiameter * Me.InletPipeDiameter / 4

                ' Calculate velocities
                If A_pipe > 0 Then
                    Me.ActualSteamVelocity = Q_VS / A_pipe
                End If
                If Me.InletArea > 0 Then
                    Me.TangentialVelocity = Q_VS / Me.InletArea
                End If
                ' Annular velocity V_AN = 4×Q_VS / (π×(D² - D_e²))
                Dim D As Double = Me.VesselDiameter
                Dim D_e As Double = Me.SteamOutletDiameter
                If D > D_e Then
                    Me.AnnularVelocity = 4 * Q_VS / (Math.PI * (D * D - D_e * D_e))
                End If

                ' Detect flow pattern
                If Me.FlowPattern = FlowPatterns.Auto Then
                    Me.DetectedFlowPattern = DetectBakerFlowPattern(W_V, W_L, rho_V, rho_L, sigma_L, mu_L, A_pipe)
                Else
                    Me.DetectedFlowPattern = Me.FlowPattern
                End If

                ' Calculate drop diameter
                Me.DropDiameter = CalculateDropDiameter(Me.ActualSteamVelocity, Q_VS, Q_L,
                                                         rho_L, sigma_L, mu_L,
                                                         Me.DetectedFlowPattern, Me.InletSteamQuality)

                ' Calculate centrifugal efficiency
                Me.CentrifugalEfficiency = CalculateCentrifugalEfficiency(D, D_e, Me.InletArea,
                                                                           Me.TangentialVelocity, Me.DropDiameter,
                                                                           rho_L, mu_V, T, Q_VS,
                                                                           Me.TotalHeight, Me.LipPosition)

                ' Calculate entrainment efficiency
                Me.EntrainmentEfficiency = CalculateEntrainmentEfficiency(Me.AnnularVelocity)

                ' Calculate overall efficiency
                Me.OverallEfficiency = Me.CentrifugalEfficiency * Me.EntrainmentEfficiency

                ' Calculate outlet steam quality
                Me.OutletSteamQuality = CalculateOutletQuality(W_V, W_L, Me.OverallEfficiency)

                ' Calculate water carryover
                Me.WaterCarryover = W_L * (1 - Me.OverallEfficiency)

                ' Calculate pressure drop
                Me.SeparatorPressureDrop = CalculatePressureDrop(Me.TangentialVelocity, rho_V,
                                                                  Me.InletArea, D_e,
                                                                  Me.EquipmentType, Me.InletPipeDiameter)

                ' Validate velocity
                ValidateVelocity()

            Catch ex As Exception
                ' Log error but don't fail the calculation
                Me.VelocityStatus = "Error in Lazalde-Crabtree calculation: " & ex.Message
                Me.VelocityInRange = False
            End Try
        End Sub

        ''' <summary>
        ''' Validates steam velocity against recommended ranges
        ''' </summary>
        Private Sub ValidateVelocity()
            Dim V_T As Double = Me.ActualSteamVelocity
            Dim minV As Double
            Dim maxV As Double

            If Me.EquipmentType = SeparatorType.Separator Then
                minV = 25
                maxV = 45
            Else
                minV = 35
                maxV = 60
            End If

            If V_T < minV Then
                Me.VelocityStatus = String.Format("WARNING: V_T = {0:F1} m/s is below recommended {1} m/s minimum", V_T, minV)
                Me.VelocityInRange = False
            ElseIf V_T > maxV Then
                Me.VelocityStatus = String.Format("WARNING: V_T = {0:F1} m/s exceeds recommended {1} m/s maximum", V_T, maxV)
                Me.VelocityInRange = False
            Else
                Me.VelocityStatus = String.Format("V_T = {0:F1} m/s is within recommended range ({1}-{2} m/s)", V_T, minV, maxV)
                Me.VelocityInRange = True
            End If
        End Sub

#End Region

    End Class

End Namespace
