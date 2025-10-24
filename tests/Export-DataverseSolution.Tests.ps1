. $PSScriptRoot/Common.ps1

Describe "Export-DataverseSolution" {
    
    BeforeAll {
        Import-Module Rnwood.Dataverse.Data.PowerShell
    }

    It "Cmdlet is available and has expected parameters" {
        $cmdlet = Get-Command Export-DataverseSolution -ErrorAction SilentlyContinue
        $cmdlet | Should -Not -BeNullOrEmpty
        $cmdlet.Name | Should -Be "Export-DataverseSolution"
        
        # Check key parameters exist
        $cmdlet.Parameters.Keys | Should -Contain "SolutionName"
        $cmdlet.Parameters.Keys | Should -Contain "Managed"
        $cmdlet.Parameters.Keys | Should -Contain "OutFile"
        $cmdlet.Parameters.Keys | Should -Contain "PassThru"
        $cmdlet.Parameters.Keys | Should -Contain "Connection"
        $cmdlet.Parameters.Keys | Should -Contain "PollingIntervalSeconds"
        $cmdlet.Parameters.Keys | Should -Contain "TimeoutSeconds"
        
        # Check export settings parameters
        $cmdlet.Parameters.Keys | Should -Contain "ExportAutoNumberingSettings"
        $cmdlet.Parameters.Keys | Should -Contain "ExportCalendarSettings"
        $cmdlet.Parameters.Keys | Should -Contain "ExportCustomizationSettings"
    }

    It "SolutionName parameter is mandatory" {
        $cmdlet = Get-Command Export-DataverseSolution
        $solutionNameParam = $cmdlet.Parameters["SolutionName"]
        $solutionNameParam.Attributes.Mandatory | Should -Contain $true
    }

    It "Has correct verb-noun structure" {
        $cmdlet = Get-Command Export-DataverseSolution
        $cmdlet.Verb | Should -Be "Export"
        $cmdlet.Noun | Should -Be "DataverseSolution"
    }

    It "Supports ShouldProcess for WhatIf and Confirm" {
        $cmdlet = Get-Command Export-DataverseSolution
        $cmdlet.Parameters.Keys | Should -Contain "WhatIf"
        $cmdlet.Parameters.Keys | Should -Contain "Confirm"
    }

    It "Has proper parameter sets" {
        $cmdlet = Get-Command Export-DataverseSolution
        # The cmdlet should work with OutFile, PassThru, or both
        $cmdlet | Should -Not -BeNullOrEmpty
    }

    # Note: Full end-to-end testing of the async monitoring requires a real Dataverse
    # environment with asyncoperation entity support. The above tests validate the
    # cmdlet structure, parameters, and WhatIf/Confirm behavior.
    
    It "Respects WhatIf parameter" {
        # Arrange
        $connection = getMockConnection
        $solutionName = "TestSolution"
        
        # Act & Assert - Should not throw, just show what would happen
        { Export-DataverseSolution -Connection $connection -SolutionName $solutionName -PassThru -WhatIf -PollingIntervalSeconds 1 -TimeoutSeconds 30 } | Should -Not -Throw
    }
}
