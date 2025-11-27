Imports System
Imports DWSIM.UnitOperations.UnitOperations

''' <summary>
''' Basic tests for the Geothermal Separator unit operation.
''' These tests verify that the separator can be instantiated and basic properties work.
''' </summary>
Public Module TestGeothermalSeparator

    Private PassCount As Integer = 0
    Private FailCount As Integer = 0

    Public Sub Main()
        Console.WriteLine("========================================")
        Console.WriteLine("Geothermal Separator Tests")
        Console.WriteLine("========================================")
        Console.WriteLine()

        ' Run all tests
        TestInstantiation()
        TestIExternalUnitOperationProperties()
        TestGetIconBitmap()
        TestGetDisplayName()
        TestGetDisplayDescription()

        ' Summary
        Console.WriteLine()
        Console.WriteLine("========================================")
        Console.WriteLine($"Tests Run: {PassCount + FailCount}")
        Console.WriteLine($"Passed: {PassCount}")
        Console.WriteLine($"Failed: {FailCount}")
        Console.WriteLine("========================================")

        If FailCount > 0 Then
            Environment.ExitCode = 1
        Else
            Environment.ExitCode = 0
        End If
    End Sub

    Private Sub TestInstantiation()
        Console.Write("Test: Instantiation... ")
        Try
            Dim separator As New GeothermalSeparator()
            If separator IsNot Nothing Then
                Console.WriteLine("PASS")
                PassCount += 1
            Else
                Console.WriteLine("FAIL - separator is Nothing")
                FailCount += 1
            End If
        Catch ex As Exception
            Console.WriteLine($"FAIL - {ex.Message}")
            FailCount += 1
        End Try
    End Sub

    Private Sub TestIExternalUnitOperationProperties()
        Console.Write("Test: IExternalUnitOperation Properties... ")
        Try
            Dim separator As New GeothermalSeparator()

            ' Test Prefix
            Dim prefix = separator.Prefix
            If String.IsNullOrEmpty(prefix) Then
                Console.WriteLine("FAIL - Prefix is empty")
                FailCount += 1
                Return
            End If

            ' Test Name
            Dim name = separator.ExternalName
            If String.IsNullOrEmpty(name) Then
                Console.WriteLine("FAIL - Name is empty")
                FailCount += 1
                Return
            End If

            ' Test Description
            Dim description = separator.ExternalDescription
            If String.IsNullOrEmpty(description) Then
                Console.WriteLine("FAIL - Description is empty")
                FailCount += 1
                Return
            End If

            Console.WriteLine($"PASS (Prefix={prefix}, Name={name})")
            PassCount += 1

        Catch ex As Exception
            Console.WriteLine($"FAIL - {ex.Message}")
            FailCount += 1
        End Try
    End Sub

    Private Sub TestGetIconBitmap()
        Console.Write("Test: GetIconBitmap... ")
        Try
            Dim separator As New GeothermalSeparator()
            Dim icon = separator.GetIconBitmap()

            If icon IsNot Nothing Then
                Console.WriteLine("PASS")
                PassCount += 1
            Else
                Console.WriteLine("FAIL - icon is Nothing")
                FailCount += 1
            End If
        Catch ex As Exception
            Console.WriteLine($"FAIL - {ex.Message}")
            FailCount += 1
        End Try
    End Sub

    Private Sub TestGetDisplayName()
        Console.Write("Test: GetDisplayName... ")
        Try
            Dim separator As New GeothermalSeparator()
            Dim displayName = separator.GetDisplayName()

            If Not String.IsNullOrEmpty(displayName) Then
                Console.WriteLine($"PASS ({displayName})")
                PassCount += 1
            Else
                Console.WriteLine("FAIL - display name is empty")
                FailCount += 1
            End If
        Catch ex As Exception
            Console.WriteLine($"FAIL - {ex.Message}")
            FailCount += 1
        End Try
    End Sub

    Private Sub TestGetDisplayDescription()
        Console.Write("Test: GetDisplayDescription... ")
        Try
            Dim separator As New GeothermalSeparator()
            Dim description = separator.GetDisplayDescription()

            If Not String.IsNullOrEmpty(description) Then
                Console.WriteLine($"PASS ({description})")
                PassCount += 1
            Else
                Console.WriteLine("FAIL - description is empty")
                FailCount += 1
            End If
        Catch ex As Exception
            Console.WriteLine($"FAIL - {ex.Message}")
            FailCount += 1
        End Try
    End Sub

End Module
