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
                f.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Float
                f.Tag = "ObjectEditor"
                Me.FlowSheet.DisplayForm(f)
            Else
                If f.IsDisposed Then
                    f = New EditingForm_GeothermalSeparator With {.SeparatorObject = Me}
                    f.ShowHint = WeifenLuo.WinFormsUI.Docking.DockState.Float
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
                    f.UpdateInfo()
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

                            ' Vessel body dimensions (taller vessel - double height ratio)
                            Dim vesselLeft = X + 0.3 * Width
                            Dim vesselRight = X + 0.7 * Width
                            Dim vesselWidth = vesselRight - vesselLeft

                            ' Main body rectangle (taller proportions)
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

            ' Vessel body dimensions (must match Draw method - taller vessel)
            Dim vesselLeft = X + 0.3 * Width
            Dim vesselRight = X + 0.7 * Width
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
            vesselLeft = GraphicObject.X + 0.3 * GraphicObject.Width
            vesselRight = GraphicObject.X + 0.7 * GraphicObject.Width
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

    End Class

End Namespace
