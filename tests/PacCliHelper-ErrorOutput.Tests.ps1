. $PSScriptRoot/Common.ps1

Describe 'PacCliHelper - Error Output Inclusion' {

    BeforeAll {
        # Ensure module is loaded
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
    }

    Context "Error message format" {
        
        It "Error messages should reference PAC CLI output inclusion" {
            # This test validates that the error handling code exists and is structured correctly
            # We're testing the pattern, not the actual PAC CLI execution since that would require
            # PAC CLI to be installed and would need to create failing scenarios
            
            # Check that the cmdlet code includes PAC CLI output in error messages
            $cmdletPath = Join-Path $PSScriptRoot ".." "Rnwood.Dataverse.Data.PowerShell.Cmdlets" "Commands"
            
            # Test Compress cmdlet includes output in errors
            $compressCode = Get-Content (Join-Path $cmdletPath "CompressDataverseSolutionFileCmdlet.cs") -Raw
            $compressCode | Should -Match "PAC CLI output:"
            $compressCode | Should -Match "ExecutePacCliWithOutput"
            
            # Test Expand cmdlet includes output in errors
            $expandCode = Get-Content (Join-Path $cmdletPath "ExpandDataverseSolutionFileCmdlet.cs") -Raw
            $expandCode | Should -Match "PAC CLI output:"
            $expandCode | Should -Match "ExecutePacCliWithOutput"
            
            # Test Export cmdlet includes output in errors
            $exportCode = Get-Content (Join-Path $cmdletPath "ExportDataverseSolutionCmdlet.cs") -Raw
            $exportCode | Should -Match "PAC CLI output:"
            $exportCode | Should -Match "ExecutePacCliWithOutput"
            
            # Test Import cmdlet includes output in errors
            $importCode = Get-Content (Join-Path $cmdletPath "ImportDataverseSolutionCmdlet.cs") -Raw
            $importCode | Should -Match "PAC CLI output:"
            $importCode | Should -Match "ExecutePacCliWithOutput"
        }
        
        It "PacCliHelper should have ExecutePacCliWithOutput method" {
            # Check that PacCliHelper.cs contains the new method
            $helperPath = Join-Path $PSScriptRoot ".." "Rnwood.Dataverse.Data.PowerShell.Cmdlets" "Commands" "PacCliHelper.cs"
            $helperCode = Get-Content $helperPath -Raw
            
            $helperCode | Should -Match "class PacCliResult"
            $helperCode | Should -Match "ExecutePacCliWithOutput"
            $helperCode | Should -Match "public int ExitCode"
            $helperCode | Should -Match "public string Output"
        }
        
        It "PacCliHelper backward compatibility method exists" {
            # Verify the obsolete wrapper method still exists for backward compatibility
            $helperPath = Join-Path $PSScriptRoot ".." "Rnwood.Dataverse.Data.PowerShell.Cmdlets" "Commands" "PacCliHelper.cs"
            $helperCode = Get-Content $helperPath -Raw
            
            $helperCode | Should -Match "public static int ExecutePacCli"
            $helperCode | Should -Match "\[Obsolete"
        }
    }
}
