. $PSScriptRoot/Common.ps1

Describe 'Invoke-DataverseSolutionUpgrade - Multiple Solutions' {

    BeforeAll {
        # Ensure module is loaded (required for parallel job execution in CI)
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)) {
            Import-Module Rnwood.Dataverse.Data.PowerShell -ErrorAction Stop
        }
    }

    Context "Multiple solution name upgrade" {
        
        It "Accepts array of solution names" {
            # Create mock connection
            $connection = getMockConnection
            
            # Attempt to upgrade with WhatIf to verify the cmdlet accepts multiple names
            # We use WhatIf so we don't actually try to execute the upgrade
            { Invoke-DataverseSolutionUpgrade -SolutionName "TestSolution1", "TestSolution2", "TestSolution3" -Connection $connection -WhatIf } | Should -Not -Throw
        }
        
        It "Accepts array of solution names via array syntax" {
            # Create mock connection
            $connection = getMockConnection
            
            # Attempt to upgrade with WhatIf using array syntax
            { Invoke-DataverseSolutionUpgrade -SolutionName @("TestSolution1", "TestSolution2", "TestSolution3") -Connection $connection -WhatIf } | Should -Not -Throw
        }
        
        It "Still accepts single solution name (backward compatibility)" {
            # Create mock connection
            $connection = getMockConnection
            
            # Attempt to upgrade single solution with WhatIf
            { Invoke-DataverseSolutionUpgrade -SolutionName "TestSolutionSingle" -Connection $connection -WhatIf } | Should -Not -Throw
        }
        
        It "Works with IfExists switch for multiple solutions" {
            # Create mock connection
            $connection = getMockConnection
            
            # Attempt to upgrade with IfExists and WhatIf
            { Invoke-DataverseSolutionUpgrade -SolutionName "TestSolution1", "TestSolution2" -IfExists -Connection $connection -WhatIf } | Should -Not -Throw
        }
    }

    Context "SolutionName parameter validation" {
        
        It "SolutionName parameter accepts string array type" {
            $paramInfo = (Get-Command Invoke-DataverseSolutionUpgrade).Parameters['SolutionName']
            $paramInfo | Should -Not -BeNullOrEmpty
            $paramInfo.ParameterType.Name | Should -Be 'String[]'
        }
        
        It "SolutionName parameter is mandatory" {
            $paramInfo = (Get-Command Invoke-DataverseSolutionUpgrade).Parameters['SolutionName']
            $paramInfo.Attributes | Where-Object { $_.TypeId.Name -eq 'ParameterAttribute' } | 
                Select-Object -First 1 -ExpandProperty Mandatory | Should -Be $true
        }
        
        It "SolutionName parameter is in position 0" {
            $paramInfo = (Get-Command Invoke-DataverseSolutionUpgrade).Parameters['SolutionName']
            $paramInfo.Attributes | Where-Object { $_.TypeId.Name -eq 'ParameterAttribute' } | 
                Select-Object -First 1 -ExpandProperty Position | Should -Be 0
        }
    }
}
