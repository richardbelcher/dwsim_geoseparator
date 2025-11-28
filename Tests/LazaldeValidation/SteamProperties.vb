Imports System
Imports SharpProp
Imports UnitsNet

''' <summary>
''' Steam/Water thermodynamic properties using CoolProp via SharpProp wrapper.
'''
''' NOTE: In the actual DWSIM unit operation, these properties will be obtained from
''' the DWSIM property package (e.g., Steam Tables, CoolProp, or other EOS).
''' This module is for standalone validation only.
'''
''' To convert to DWSIM:
'''   - Use MaterialStream.Phases(phase).Properties.density instead of GetDensity()
'''   - Use MaterialStream.Phases(phase).Properties.viscosity instead of GetViscosity()
'''   - Use MaterialStream.Phases(phase).Properties.surfaceTension instead of GetSurfaceTension()
'''   - DWSIM handles phase equilibrium automatically via Flash calculations
''' </summary>
Public Module SteamProperties

    ''' <summary>
    ''' Get saturated steam (vapor) properties at given pressure
    ''' </summary>
    Public Function GetSaturatedVaporProperties(pressure_Pa As Double) As SteamState
        Dim state As New SteamState()
        state.Pressure = pressure_Pa

        Try
            ' Create water fluid at saturated vapor conditions (quality = 1)
            Dim water As New Fluid(FluidsList.Water)
            water = water.WithState(
                Input.Pressure(Pressure.FromPascals(pressure_Pa)),
                Input.Quality(Ratio.FromPercent(100))  ' Saturated vapor
            )

            state.Temperature = water.Temperature.DegreesCelsius
            state.Density = water.Density.KilogramsPerCubicMeter
            If water.DynamicViscosity.HasValue Then
                state.DynamicViscosity = water.DynamicViscosity.Value.PascalSeconds
            End If
            state.Enthalpy = water.Enthalpy.JoulesPerKilogram
            state.Entropy = water.Entropy.JoulesPerKilogramKelvin
            state.IsValid = True

        Catch ex As Exception
            Console.WriteLine($"  [CoolProp Error - Vapor]: {ex.Message}")
            state.IsValid = False
        End Try

        Return state
    End Function

    ''' <summary>
    ''' Get saturated liquid (water) properties at given pressure
    ''' </summary>
    Public Function GetSaturatedLiquidProperties(pressure_Pa As Double) As SteamState
        Dim state As New SteamState()
        state.Pressure = pressure_Pa

        Try
            ' Create water fluid at saturated liquid conditions (quality = 0)
            Dim water As New Fluid(FluidsList.Water)
            water = water.WithState(
                Input.Pressure(Pressure.FromPascals(pressure_Pa)),
                Input.Quality(Ratio.FromPercent(0))  ' Saturated liquid
            )

            state.Temperature = water.Temperature.DegreesCelsius
            state.Density = water.Density.KilogramsPerCubicMeter
            If water.DynamicViscosity.HasValue Then
                state.DynamicViscosity = water.DynamicViscosity.Value.PascalSeconds
            End If
            state.Enthalpy = water.Enthalpy.JoulesPerKilogram
            state.Entropy = water.Entropy.JoulesPerKilogramKelvin
            state.IsValid = True

        Catch ex As Exception
            Console.WriteLine($"  [CoolProp Error - Liquid]: {ex.Message}")
            state.IsValid = False
        End Try

        Return state
    End Function

    ''' <summary>
    ''' Get surface tension of water at given temperature
    ''' Using Vargaftik correlation (Eq. 24 from Lazalde-Crabtree)
    ''' CoolProp also provides surface tension but Vargaftik is the paper reference
    ''' </summary>
    Public Function GetSurfaceTension(temperature_C As Double) As Double
        ' Eq. 24: σ = Y × [(Tc - T)/Tc]^k × [1 + b×(Tc - T)/Tc]
        Const Tc As Double = 647.15      ' Critical temperature [K]
        Const Y As Double = 0.2358       ' 235.8×10^-3 N/m
        Const b As Double = -0.625       ' Coefficient
        Const k As Double = 1.256        ' Exponent

        Dim T_K As Double = temperature_C + 273.15
        Dim tau As Double = (Tc - T_K) / Tc
        Dim sigma As Double = Y * Math.Pow(tau, k) * (1 + b * tau)

        Return sigma  ' N/m
    End Function

    ''' <summary>
    ''' Get surface tension from CoolProp (alternative to Vargaftik)
    ''' </summary>
    Public Function GetSurfaceTensionCoolProp(pressure_Pa As Double) As Double
        Try
            Dim water As New Fluid(FluidsList.Water)
            water = water.WithState(
                Input.Pressure(Pressure.FromPascals(pressure_Pa)),
                Input.Quality(Ratio.FromPercent(0))  ' Saturated liquid
            )
            If water.SurfaceTension.HasValue Then
                Return water.SurfaceTension.Value.NewtonsPerMeter
            Else
                Return 0.05  ' Fallback
            End If
        Catch ex As Exception
            Return 0.05  ' Default fallback ~50 mN/m
        End Try
    End Function

    ''' <summary>
    ''' Get all properties needed for Lazalde-Crabtree separator calculation
    ''' </summary>
    Public Function GetSeparatorProperties(pressure_Pa As Double) As SeparatorFluidProperties
        Dim props As New SeparatorFluidProperties()
        props.Pressure = pressure_Pa

        ' Get saturated vapor properties
        Dim vapor = GetSaturatedVaporProperties(pressure_Pa)
        props.VaporDensity = vapor.Density
        props.VaporViscosity = vapor.DynamicViscosity
        props.Temperature = vapor.Temperature
        props.VaporEnthalpy = vapor.Enthalpy

        ' Get saturated liquid properties
        Dim liquid = GetSaturatedLiquidProperties(pressure_Pa)
        props.LiquidDensity = liquid.Density
        props.LiquidViscosity = liquid.DynamicViscosity
        props.LiquidEnthalpy = liquid.Enthalpy

        ' Get surface tension (using Vargaftik correlation for consistency with paper)
        props.SurfaceTension = GetSurfaceTension(props.Temperature)

        props.IsValid = vapor.IsValid AndAlso liquid.IsValid

        Return props
    End Function

    ''' <summary>
    ''' Print comparison of CoolProp vs paper values
    ''' </summary>
    Public Sub PrintPropertyComparison(pressure_Pa As Double, paperValues As SeparatorFluidProperties)
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗")
        Console.WriteLine("║        COOLPROP vs PAPER PROPERTY COMPARISON                 ║")
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝")
        Console.WriteLine()
        Console.WriteLine("  NOTE: In DWSIM, use MaterialStream.Phases(phase).Properties")
        Console.WriteLine("        instead of CoolProp for thermodynamic properties.")
        Console.WriteLine()

        Dim coolprop = GetSeparatorProperties(pressure_Pa)

        Console.WriteLine($"  Pressure: {pressure_Pa / 1000:F1} kPa ({pressure_Pa / 100000:F2} bar)")
        Console.WriteLine()
        Console.WriteLine("  Property              │ CoolProp      │ Paper         │ Diff %")
        Console.WriteLine("  ──────────────────────┼───────────────┼───────────────┼────────")
        Console.WriteLine($"  T_sat (°C)            │ {coolprop.Temperature,10:F2}    │ {paperValues.Temperature,10:F2}    │ {(coolprop.Temperature - paperValues.Temperature) / paperValues.Temperature * 100:F2}%")
        Console.WriteLine($"  ρ_V (kg/m³)           │ {coolprop.VaporDensity,10:F4}  │ {paperValues.VaporDensity,10:F4}  │ {(coolprop.VaporDensity - paperValues.VaporDensity) / paperValues.VaporDensity * 100:F2}%")
        Console.WriteLine($"  ρ_L (kg/m³)           │ {coolprop.LiquidDensity,10:F2}  │ {paperValues.LiquidDensity,10:F2}  │ {(coolprop.LiquidDensity - paperValues.LiquidDensity) / paperValues.LiquidDensity * 100:F2}%")
        Console.WriteLine($"  μ_V (Pa·s)            │ {coolprop.VaporViscosity,10:E3} │ {paperValues.VaporViscosity,10:E3} │ {(coolprop.VaporViscosity - paperValues.VaporViscosity) / paperValues.VaporViscosity * 100:F2}%")
        Console.WriteLine($"  μ_L (Pa·s)            │ {coolprop.LiquidViscosity,10:E3} │ {paperValues.LiquidViscosity,10:E3} │ {(coolprop.LiquidViscosity - paperValues.LiquidViscosity) / paperValues.LiquidViscosity * 100:F2}%")
        Console.WriteLine($"  σ (N/m)               │ {coolprop.SurfaceTension,10:F6} │ {paperValues.SurfaceTension,10:F6} │ {(coolprop.SurfaceTension - paperValues.SurfaceTension) / paperValues.SurfaceTension * 100:F2}%")
        Console.WriteLine()
    End Sub

End Module

''' <summary>
''' State of steam/water at a given condition
''' </summary>
Public Class SteamState
    Public Property Pressure As Double          ' Pa
    Public Property Temperature As Double       ' °C
    Public Property Density As Double           ' kg/m³
    Public Property DynamicViscosity As Double  ' Pa·s
    Public Property Enthalpy As Double          ' J/kg
    Public Property Entropy As Double           ' J/(kg·K)
    Public Property IsValid As Boolean = False
End Class

''' <summary>
''' All fluid properties needed for separator calculation
''' </summary>
Public Class SeparatorFluidProperties
    Public Property Pressure As Double          ' Pa
    Public Property Temperature As Double       ' °C

    ' Vapor properties
    Public Property VaporDensity As Double      ' kg/m³
    Public Property VaporViscosity As Double    ' Pa·s
    Public Property VaporEnthalpy As Double     ' J/kg

    ' Liquid properties
    Public Property LiquidDensity As Double     ' kg/m³
    Public Property LiquidViscosity As Double   ' Pa·s
    Public Property LiquidEnthalpy As Double    ' J/kg

    ' Interface property
    Public Property SurfaceTension As Double    ' N/m

    Public Property IsValid As Boolean = False
End Class
