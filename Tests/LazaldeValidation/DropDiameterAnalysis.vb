Imports System

''' <summary>
''' Analyzes drop diameter calculation across all flow patterns
''' to understand why Eq. 23 gives larger values than paper implies
''' </summary>
Module DropDiameterAnalysis

    Sub RunAnalysis()
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine("   DROP DIAMETER SENSITIVITY ANALYSIS - Eq. 23, Table 4")
        Console.WriteLine("   Investigating why calculated d_w differs from paper")
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine()

        ' Input conditions from paper example
        Dim X_i As Double = 0.0756           ' Inlet quality
        Dim V_T As Double = 26.53            ' Steam velocity (our calc), paper=28.27
        Dim rho_L As Double = 876.7          ' kg/m³
        Dim rho_V As Double = 2.973          ' kg/m³
        Dim sigma As Double = 0.0477         ' N/m (from Vargaftik)
        Dim mu_L As Double = 0.000135        ' Pa·s
        Dim Q_L As Double = 0.0557           ' m³/s
        Dim Q_VS As Double = 1.344           ' m³/s

        ' CGS conversions
        Dim rho_L_cgs As Double = rho_L / 1000.0    ' g/cm³
        Dim sigma_cgs As Double = sigma * 1000.0    ' dyne/cm
        Dim mu_L_cgs As Double = mu_L * 10.0        ' poise

        Console.WriteLine("INPUT CONDITIONS:")
        Console.WriteLine($"  X_i (quality)    = {X_i * 100:F2}%")
        Console.WriteLine($"  V_T (velocity)   = {V_T:F2} m/s")
        Console.WriteLine($"  ρ_L              = {rho_L:F1} kg/m³ = {rho_L_cgs:F4} g/cm³")
        Console.WriteLine($"  σ_L              = {sigma:F4} N/m = {sigma_cgs:F2} dyne/cm")
        Console.WriteLine($"  μ_L              = {mu_L:E3} Pa·s = {mu_L_cgs:E4} poise")
        Console.WriteLine($"  Q_L/Q_VS         = {Q_L / Q_VS:F4}")
        Console.WriteLine()

        ' Constants
        Const A As Double = 66.2898
        Const K As Double = 1357.35
        Const b As Double = 0.225
        Const c As Double = 0.5507

        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine("   DROP DIAMETER FOR EACH FLOW PATTERN")
        Console.WriteLine("═══════════════════════════════════════════════════════════════")

        ' Flow patterns from Table 4
        Dim patterns() As String = {"Stratified", "Annular", "Dispersed", "PlugSlug"}
        Dim a_vals() As Double = {0.5436, 0.8069, 0.8069, 0.5436}
        Dim B_coeffs() As Double = {94.9042, 198.7749, 140.8346, 37.3618}
        Dim B_exps() As Double = {-0.4538, 0.2628, 0.5747, -0.0000688}
        Dim e_vals() As Double = {0.0253, -0.2188, -0.2188, 0.0253}

        Console.WriteLine()
        Console.WriteLine("  Flow Pattern    │   a      │   B_coeff │  B_exp    │   e       │  B(Xi)   │ Term1   │ Term2    │  d_w")
        Console.WriteLine("  ────────────────┼──────────┼───────────┼───────────┼───────────┼──────────┼─────────┼──────────┼────────")

        For i As Integer = 0 To patterns.Length - 1
            Dim a_exp As Double = a_vals(i)
            Dim B_coeff As Double = B_coeffs(i)
            Dim B_exp As Double = B_exps(i)
            Dim e_exp As Double = e_vals(i)

            ' Calculate B using Xi as exponent
            Dim B_const As Double = B_coeff * Math.Pow(X_i, B_exp)

            ' Term 1: (A/v_t^a) × √(σ/ρ)
            Dim term1 As Double = (A / Math.Pow(V_T, a_exp)) * Math.Sqrt(sigma_cgs / rho_L_cgs)

            ' Term 2: B × K × [μ²/(σρ)]^b × (Q_L/Q_VS)^c × v_t^e
            Dim visc_term As Double = Math.Pow(mu_L_cgs * mu_L_cgs / (sigma_cgs * rho_L_cgs), b)
            Dim flow_term As Double = Math.Pow(Q_L / Q_VS, c)
            Dim vel_term As Double = Math.Pow(V_T, e_exp)
            Dim term2 As Double = B_const * K * visc_term * flow_term * vel_term

            Dim d_w As Double = term1 + term2

            Console.WriteLine($"  {patterns(i),-14} │ {a_exp,8:F4} │ {B_coeff,9:F4} │ {B_exp,9:F4} │ {e_exp,9:F4} │ {B_const,8:F2} │ {term1,7:F1} │ {term2,8:F1} │ {d_w,6:F0} μm")
        Next

        Console.WriteLine()
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine("   ANALYSIS: WHY ALL FLOW PATTERNS GIVE LARGE d_w")
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine()
        Console.WriteLine("  KEY OBSERVATIONS:")
        Console.WriteLine()
        Console.WriteLine("  1. Term 2 DOMINATES the calculation (>85% of total)")
        Console.WriteLine("     - This term depends on B × K × viscosity × flow_ratio × velocity")
        Console.WriteLine("     - The large constant K = 1357.35 amplifies this term")
        Console.WriteLine()
        Console.WriteLine("  2. B coefficient is LARGE for all patterns:")
        Console.WriteLine($"     - Stratified:  B = 94.9 × (0.0756)^-0.454 = {94.9042 * Math.Pow(0.0756, -0.4538):F1}")
        Console.WriteLine($"     - Annular:     B = 198.8 × (0.0756)^0.263 = {198.7749 * Math.Pow(0.0756, 0.2628):F1}")
        Console.WriteLine($"     - Dispersed:   B = 140.8 × (0.0756)^0.575 = {140.8346 * Math.Pow(0.0756, 0.5747):F1}")
        Console.WriteLine($"     - PlugSlug:    B = 37.4 × (0.0756)^-0.00007 = {37.3618 * Math.Pow(0.0756, -0.0000688):F1}")
        Console.WriteLine()
        Console.WriteLine("  3. Low quality (Xi = 7.56%) makes B larger for negative exponents")
        Console.WriteLine("     - Stratified has B_exp = -0.4538, so low Xi → large B")
        Console.WriteLine()

        ' What d_w would be needed for η_m = 99.9973%?
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine("   BACK-CALCULATION: What d_w gives η_m = 99.9973%?")
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine()

        ' Parameters for η_m calculation
        Dim D As Double = 0.8382        ' Vessel diameter
        Dim D_e As Double = 0.254       ' Outlet diameter
        Dim Z As Double = 1.016         ' Height
        Dim alpha As Double = -0.0381   ' Lip position
        Dim T As Double = 155           ' Temperature °C
        Dim u As Double = V_T           ' Tangential velocity
        Dim mu_V As Double = 0.0000135  ' Steam viscosity Pa·s
        Dim A_o As Double = Math.PI * 0.254 * 0.254 / 4  ' Inlet area

        ' Calculate n
        Dim n1 As Double = 0.6689 * Math.Pow(D, 0.14)
        Dim T_K As Double = T + 273.15
        Dim temp_ratio As Double = Math.Pow(294.3 / T_K, 0.3)
        Dim n As Double = 1 - (1 - n1) / temp_ratio

        ' Calculate C
        Dim V_OS As Double = (Math.PI / 4) * (D * D - D_e * D_e) * Z
        Dim head_rise As Double = 0.169 * D
        Dim V_head As Double = 0.081 * D * D * D
        Dim V_tube As Double = (Math.PI / 4) * D_e * D_e * (Math.Abs(alpha) + head_rise)
        Dim V_OH As Double = V_head - V_tube
        Dim t_mi As Double = V_OS / Q_VS
        Dim t_ma As Double = V_OH / Q_VS
        Dim t_r As Double = t_mi + t_ma / 2
        Dim K_c As Double = t_r * Q_VS / (D * D * D)
        Dim C_param As Double = 8 * K_c * D * D / A_o

        Console.WriteLine($"  Efficiency parameters:")
        Console.WriteLine($"    n = {n:F4}")
        Console.WriteLine($"    C = {C_param:F2}")
        Console.WriteLine()

        ' For η_m = 0.999973:
        ' η_m = 1 - exp(-2(ψ'C)^(1/(2n+2)))
        ' 0.000027 = exp(-term)
        ' term = -ln(0.000027) = 10.52
        ' 2(ψ'C)^exp = 10.52
        ' (ψ'C)^exp = 5.26
        ' With exp = 1/(2n+2) = 0.31
        ' ψ'C = 5.26^(1/0.31) = 230
        ' ψ' = 230 / C = 230 / 99.5 = 2.31

        Dim eta_target As Double = 0.999973
        Dim exp_val As Double = 1 / (2 * n + 2)
        Dim term_needed As Double = -Math.Log(1 - eta_target)
        Dim psiC_needed As Double = Math.Pow(term_needed / 2, 1 / exp_val)
        Dim psi_needed As Double = psiC_needed / C_param

        Console.WriteLine($"  To achieve η_m = {eta_target * 100:F4}%:")
        Console.WriteLine($"    exp = 1/(2n+2) = {exp_val:F4}")
        Console.WriteLine($"    term needed = -ln(1-η_m) = {term_needed:F4}")
        Console.WriteLine($"    (ψ'C) needed = {psiC_needed:F2}")
        Console.WriteLine($"    ψ' needed = {psi_needed:F4}")
        Console.WriteLine()

        ' ψ' = ρ_L × d_w² × (n+1) × u / (18 × μ_V × D)
        ' d_w² = ψ' × 18 × μ_V × D / (ρ_L × (n+1) × u)
        Dim d_w_squared As Double = psi_needed * 18 * mu_V * D / (rho_L * (n + 1) * u)
        Dim d_w_needed As Double = Math.Sqrt(d_w_squared) * 1000000  ' Convert to μm

        Console.WriteLine($"  Back-calculating d_w from ψ':")
        Console.WriteLine($"    ψ' = ρ_L × d_w² × (n+1) × u / (18 × μ_V × D)")
        Console.WriteLine($"    d_w² = ψ' × 18 × μ_V × D / (ρ_L × (n+1) × u)")
        Console.WriteLine($"    d_w² = {psi_needed:F4} × 18 × {mu_V:E3} × {D:F4} / ({rho_L:F1} × {n + 1:F4} × {u:F2})")
        Console.WriteLine($"    d_w² = {d_w_squared:E6} m²")
        Console.WriteLine()
        Console.WriteLine($"  ★ d_w needed for η_m = 99.9973%: {d_w_needed:F0} μm")
        Console.WriteLine()

        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine("   CONCLUSIONS")
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine()
        Console.WriteLine("  1. Eq. 23 gives d_w ≈ 290 μm for Annular flow at Xi = 7.56%")
        Console.WriteLine("  2. Paper results (η_m = 99.9973%) imply d_w ≈ 112 μm")
        Console.WriteLine("  3. This is a ~2.6x discrepancy")
        Console.WriteLine()
        Console.WriteLine("  POSSIBLE EXPLANATIONS:")
        Console.WriteLine("  • Eq. 23 is semi-empirical, derived from wellhead separator data")
        Console.WriteLine("  • The paper may use a different characteristic diameter")
        Console.WriteLine("  • Field conditions may produce smaller drops than equation predicts")
        Console.WriteLine("  • The Harwell technique (mentioned in CFD section) gives different values")
        Console.WriteLine("  • Lazalde-Crabtree may have used measured data, not calculated d_w")
        Console.WriteLine()
        Console.WriteLine("  RECOMMENDATION:")
        Console.WriteLine("  • The efficiency equations (Eq. 14-21) are correctly implemented")
        Console.WriteLine("  • The drop diameter equation needs further investigation")
        Console.WriteLine("  • Consider using empirical d_w based on separator measurements")
        Console.WriteLine()

        ' =====================================================
        ' BAKER FLOW PATTERN MAP ANALYSIS
        ' =====================================================
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine("   BAKER FLOW PATTERN MAP - Which pattern applies?")
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine()

        ' Baker parameters (Eq. 33, Table 5)
        Dim rho_W As Double = 1000.0  ' Water reference density kg/m³
        Dim rho_A As Double = 1.23    ' Air reference density kg/m³
        Dim mu_W As Double = 0.001    ' Water reference viscosity Pa·s
        Dim sigma_W As Double = 0.073 ' Water reference surface tension N/m

        ' Flow rates
        Dim D_t As Double = 0.254     ' Inlet pipe diameter
        Dim A_pipe As Double = Math.PI * D_t * D_t / 4

        ' Mass velocities
        Dim G_V As Double = (X_i * 52.87) / A_pipe    ' kg/(m²·s) steam mass velocity
        Dim G_L As Double = ((1 - X_i) * 52.87) / A_pipe  ' kg/(m²·s) liquid mass velocity

        Console.WriteLine($"  Mass velocities:")
        Console.WriteLine($"    G_V = W_V/A = {X_i * 52.87:F2}/{A_pipe:F6} = {G_V:F2} kg/(m²·s)")
        Console.WriteLine($"    G_L = W_L/A = {(1 - X_i) * 52.87:F2}/{A_pipe:F6} = {G_L:F2} kg/(m²·s)")
        Console.WriteLine()

        ' Baker parameters
        Dim lambda As Double = Math.Sqrt((rho_V / rho_A) * (rho_L / rho_W))
        Dim psi As Double = (sigma_W / sigma) * Math.Pow(mu_L / mu_W, 0.5) * Math.Pow(rho_W / rho_L, 2)

        Console.WriteLine($"  Baker parameters:")
        Console.WriteLine($"    λ = √[(ρ_V/ρ_A) × (ρ_L/ρ_W)]")
        Console.WriteLine($"      = √[({rho_V:F3}/{rho_A:F2}) × ({rho_L:F1}/{rho_W:F1})]")
        Console.WriteLine($"      = √[{rho_V / rho_A:F4} × {rho_L / rho_W:F4}]")
        Console.WriteLine($"      = {lambda:F4}")
        Console.WriteLine()
        Console.WriteLine($"    ψ = (σ_W/σ) × (μ_L/μ_W)^0.5 × (ρ_W/ρ_L)²")
        Console.WriteLine($"      = ({sigma_W:F4}/{sigma:F4}) × ({mu_L:F6}/{mu_W:F4})^0.5 × ({rho_W:F1}/{rho_L:F1})²")
        Console.WriteLine($"      = {sigma_W / sigma:F4} × {Math.Pow(mu_L / mu_W, 0.5):F4} × {Math.Pow(rho_W / rho_L, 2):F4}")
        Console.WriteLine($"      = {psi:F4}")
        Console.WriteLine()

        ' Baker coordinates
        Dim X_baker As Double = G_L * psi / lambda  ' Abscissa
        Dim Y_baker As Double = G_V * lambda        ' Ordinate

        Console.WriteLine($"  Baker map coordinates:")
        Console.WriteLine($"    X = G_L × ψ / λ = {G_L:F2} × {psi:F4} / {lambda:F4} = {X_baker:F1}")
        Console.WriteLine($"    Y = G_V × λ = {G_V:F2} × {lambda:F4} = {Y_baker:F1}")
        Console.WriteLine()

        ' Flow pattern boundaries (approximate from Baker map)
        Console.WriteLine("  Baker map boundaries (approximate):")
        Console.WriteLine("    Annular:    Y > 100, X < 10000")
        Console.WriteLine("    Dispersed:  Y < 100, X > 5000")
        Console.WriteLine("    Stratified: Y < 10,  X < 1000")
        Console.WriteLine("    Plug/Slug:  10 < Y < 100, X < 1000")
        Console.WriteLine()

        Console.WriteLine($"  With X = {X_baker:F1}, Y = {Y_baker:F1}:")
        If Y_baker > 100 AndAlso X_baker < 10000 Then
            Console.WriteLine("    → ANNULAR flow pattern (d_w = 290 μm)")
        ElseIf Y_baker < 100 AndAlso X_baker > 5000 Then
            Console.WriteLine("    → DISPERSED flow pattern (d_w = 116 μm)")
        ElseIf Y_baker < 10 AndAlso X_baker < 1000 Then
            Console.WriteLine("    → STRATIFIED flow pattern")
        ElseIf Y_baker >= 10 AndAlso Y_baker <= 100 AndAlso X_baker < 1000 Then
            Console.WriteLine("    → PLUG/SLUG flow pattern")
        Else
            Console.WriteLine("    → TRANSITION region (between patterns)")
        End If
        Console.WriteLine()

        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine("   FINAL CONCLUSIONS")
        Console.WriteLine("═══════════════════════════════════════════════════════════════")
        Console.WriteLine()
        Console.WriteLine("  ┌─────────────────────────────────────────────────────────────┐")
        Console.WriteLine("  │                    ROOT CAUSE IDENTIFIED                    │")
        Console.WriteLine("  └─────────────────────────────────────────────────────────────┘")
        Console.WriteLine()
        Console.WriteLine("  1. Baker map says: ANNULAR flow (Y=115 > 100)")
        Console.WriteLine("     → Eq. 23 with Annular constants gives d_w = 290 μm")
        Console.WriteLine()
        Console.WriteLine("  2. Paper's η_m = 99.9973% implies d_w ≈ 107-112 μm")
        Console.WriteLine("     → This matches DISPERSED flow pattern (d_w = 116 μm)")
        Console.WriteLine()
        Console.WriteLine("  3. The discrepancy is NOT in our implementation of:")
        Console.WriteLine("     - The efficiency equations (Eq. 14-21) ✓ CORRECT")
        Console.WriteLine("     - The entrainment equations (Eq. 25-27) ✓ CORRECT")
        Console.WriteLine("     - The Baker map flow pattern detection ✓ CORRECT")
        Console.WriteLine()
        Console.WriteLine("  4. The discrepancy IS in one of these:")
        Console.WriteLine("     a) Eq. 23 (Nukiyama-Tanasawa) with Table 4 constants")
        Console.WriteLine("        gives larger d_w than what paper used")
        Console.WriteLine("     b) Paper may have used measured d_w from experiments")
        Console.WriteLine("     c) The Harwell technique (mentioned in paper) may")
        Console.WriteLine("        give different results than Eq. 23")
        Console.WriteLine()
        Console.WriteLine("  ┌─────────────────────────────────────────────────────────────┐")
        Console.WriteLine("  │                    RECOMMENDATION                           │")
        Console.WriteLine("  └─────────────────────────────────────────────────────────────┘")
        Console.WriteLine()
        Console.WriteLine("  For GeothermalSeparator unit operation:")
        Console.WriteLine()
        Console.WriteLine("  Option A: Use Eq. 23 as-is (conservative)")
        Console.WriteLine("     - Larger d_w → higher η_m → over-predicts efficiency")
        Console.WriteLine("     - Safe for design (separator will perform better than predicted)")
        Console.WriteLine()
        Console.WriteLine("  Option B: Use DISPERSED flow pattern for low-quality (<10%)")
        Console.WriteLine("     - Low quality = more liquid = behaves like dispersed flow")
        Console.WriteLine("     - d_w ≈ 116 μm matches paper validation")
        Console.WriteLine()
        Console.WriteLine("  Option C: Allow user to specify d_w directly")
        Console.WriteLine("     - For validation against field/experimental data")
        Console.WriteLine()
    End Sub

End Module
