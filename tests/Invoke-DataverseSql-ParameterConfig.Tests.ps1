. $PSScriptRoot/Common.ps1

Describe 'Invoke-DataverseSql - UseTdsEndpoint Parameter Configuration' {
    Context 'Parameter Initialization' {
        It "Cmdlet has UseTdsEndpoint parameter defined" {
            $connection = getMockConnection
            
            # Get the cmdlet type
            $cmdlet = Get-Command Invoke-DataverseSql
            
            # Verify the parameter exists
            $cmdlet.Parameters.Keys | Should -Contain 'UseTdsEndpoint'
            
            # Verify it's a switch parameter
            $param = $cmdlet.Parameters['UseTdsEndpoint']
            $param.ParameterType.Name | Should -Be 'SwitchParameter'
        }
        
        It "UseTdsEndpoint parameter has correct help text" {
            $help = Get-Help Invoke-DataverseSql -Parameter UseTdsEndpoint
            
            # Verify help exists for the parameter
            $help | Should -Not -BeNullOrEmpty
            $help.name | Should -Be 'UseTdsEndpoint'
        }
        
        It "Code properly initializes SQL connection without redundant assignments" {
            # This test validates that the fix was applied correctly
            # Read the source file and verify the redundant line was removed
            
            $repoRoot = Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent
            $cmdletPath = Get-ChildItem -Path $repoRoot -Recurse -Filter "InvokeDataverseSqlCmdlet.cs" -ErrorAction SilentlyContinue | Select-Object -First 1
            
            if ($cmdletPath -and (Test-Path $cmdletPath.FullName)) {
                $content = Get-Content $cmdletPath.FullName -Raw
                
                # The fix removes the redundant "UseTDSEndpoint = false" line
                # Check that we only have ONE assignment to UseTDSEndpoint in BeginProcessing
                $beginProcessingSection = $content -split 'protected override void BeginProcessing\(\)' | Select-Object -Last 1
                $beginProcessingSection = $beginProcessingSection -split 'protected override void EndProcessing\(\)' | Select-Object -First 1
                
                # Count occurrences of "UseTDSEndpoint =" in BeginProcessing
                $assignments = ([regex]::Matches($beginProcessingSection, 'UseTDSEndpoint\s*=')).Count
                
                # Should only have ONE assignment now (removed the redundant false assignment)
                $assignments | Should -Be 1 -Because "There should only be one assignment to UseTDSEndpoint (the one from the parameter)"
                
                # Verify the line "UseTDSEndpoint = false" was removed
                $beginProcessingSection | Should -Not -Match 'UseTDSEndpoint\s*=\s*false' -Because "The redundant 'UseTDSEndpoint = false' assignment should be removed"
                
                # Verify the correct assignment exists
                $beginProcessingSection | Should -Match 'UseTDSEndpoint\s*=\s*UseTdsEndpoint' -Because "The correct assignment from parameter should exist"
            }
            else {
                Set-ItResult -Skipped -Because "Source file not found. Repo root: $repoRoot"
            }
        }
    }
}
