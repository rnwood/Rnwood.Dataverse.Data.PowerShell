. $PSScriptRoot/Common.ps1

Describe "Import-DataverseSolution" {
    
    BeforeAll {
        Import-Module Rnwood.Dataverse.Data.PowerShell
    }

    It "Cmdlet is available and has expected parameters" {
        $cmdlet = Get-Command Import-DataverseSolution -ErrorAction SilentlyContinue
        $cmdlet | Should -Not -BeNullOrEmpty
        $cmdlet.Name | Should -Be "Import-DataverseSolution"
        
        # Check key parameters exist
        $cmdlet.Parameters.Keys | Should -Contain "InFile"
        $cmdlet.Parameters.Keys | Should -Contain "SolutionFile"
        $cmdlet.Parameters.Keys | Should -Contain "OverwriteUnmanagedCustomizations"
        $cmdlet.Parameters.Keys | Should -Contain "PublishWorkflows"
        $cmdlet.Parameters.Keys | Should -Contain "HoldingSolution"
        $cmdlet.Parameters.Keys | Should -Contain "ConnectionReferences"
        $cmdlet.Parameters.Keys | Should -Contain "EnvironmentVariables"
        $cmdlet.Parameters.Keys | Should -Contain "Connection"
        $cmdlet.Parameters.Keys | Should -Contain "PollingIntervalSeconds"
        $cmdlet.Parameters.Keys | Should -Contain "TimeoutSeconds"
    }

    It "InFile parameter is mandatory in FromFile parameter set" {
        $cmdlet = Get-Command Import-DataverseSolution
        $inFileParam = $cmdlet.Parameters["InFile"]
        $inFileParam.Attributes.Mandatory | Should -Contain $true
    }

    It "SolutionFile parameter is mandatory in FromBytes parameter set" {
        $cmdlet = Get-Command Import-DataverseSolution
        $solutionFileParam = $cmdlet.Parameters["SolutionFile"]
        $solutionFileParam.Attributes.Mandatory | Should -Contain $true
    }

    It "Has correct verb-noun structure" {
        $cmdlet = Get-Command Import-DataverseSolution
        $cmdlet.Verb | Should -Be "Import"
        $cmdlet.Noun | Should -Be "DataverseSolution"
    }

    It "Supports ShouldProcess for WhatIf and Confirm" {
        $cmdlet = Get-Command Import-DataverseSolution
        $cmdlet.Parameters.Keys | Should -Contain "WhatIf"
        $cmdlet.Parameters.Keys | Should -Contain "Confirm"
    }

    It "ConnectionReferences parameter accepts Hashtable" {
        $cmdlet = Get-Command Import-DataverseSolution
        $connRefParam = $cmdlet.Parameters["ConnectionReferences"]
        $connRefParam.ParameterType.Name | Should -Be "Hashtable"
    }

    It "EnvironmentVariables parameter accepts Hashtable" {
        $cmdlet = Get-Command Import-DataverseSolution
        $envVarParam = $cmdlet.Parameters["EnvironmentVariables"]
        $envVarParam.ParameterType.Name | Should -Be "Hashtable"
    }

    # Note: Full end-to-end testing of the async monitoring requires a real Dataverse
    # environment with asyncoperation entity support. The above tests validate the
    # cmdlet structure, parameters, and WhatIf/Confirm behavior.
    
    It "Respects WhatIf parameter" {
        # Arrange
        $connection = getMockConnection
        $tempFile = [System.IO.Path]::GetTempFileName()
        [System.IO.File]::WriteAllBytes($tempFile, [byte[]](1,2,3,4,5))
        
        try {
            # Act & Assert - Should not throw, just show what would happen
            { Import-DataverseSolution -Connection $connection -InFile $tempFile -WhatIf -PollingIntervalSeconds 1 -TimeoutSeconds 30 } | Should -Not -Throw
        }
        finally {
            if (Test-Path $tempFile) {
                Remove-Item $tempFile -Force
            }
        }
    }
}
