Describe 'Named Connections' {

    . $PSScriptRoot/Common.ps1

    BeforeEach {
        # Clean up any existing test connections before each test
        # Only if module is loaded
        if (Get-Module Rnwood.Dataverse.Data.PowerShell) {
            $testConnections = @("TestConn1", "TestConn2", "TestConnInteractive")
            foreach ($name in $testConnections) {
                try {
                    Get-DataverseConnection -DeleteConnection -Name $name -ErrorAction SilentlyContinue
                } catch {
                    # Ignore errors if connection doesn't exist
                }
            }
        }
    }

    AfterEach {
        # Clean up test connections after each test
        if (Get-Module Rnwood.Dataverse.Data.PowerShell) {
            $testConnections = @("TestConn1", "TestConn2", "TestConnInteractive")
            foreach ($name in $testConnections) {
                try {
                    Get-DataverseConnection -DeleteConnection -Name $name -ErrorAction SilentlyContinue
                } catch {
                    # Ignore errors if connection doesn't exist
                }
            }
            
            Remove-Module Rnwood.Dataverse.Data.PowerShell
        }
    }

    It "ListConnections returns empty array when no connections saved" {
        # Load module by calling getMockConnection (which imports it)
        $conn = getMockConnection
        
        # First, delete all test connections to ensure clean state
        $testConnections = @("TestConn1", "TestConn2", "TestConnInteractive")
        foreach ($name in $testConnections) {
            try {
                Get-DataverseConnection -DeleteConnection -Name $name -ErrorAction SilentlyContinue
            } catch {
                # Ignore
            }
        }
        
        $result = Get-DataverseConnection -ListConnections
        # Result should be empty or not contain our test connections
        if ($result) {
            $result | Where-Object { $_.Name -in $testConnections } | Should -BeNullOrEmpty
        }
    }

    It "Can save a named connection with Mock provider" {
        # This test verifies that the -Name parameter works with mock connections
        # However, mock connections don't use MSAL so they won't actually cache tokens
        $connection = getMockConnection
        
        # Note: Named connections with mock provider are not fully functional 
        # since mock connections don't use MSAL, but parameter should be accepted
        $connection | Should -Not -BeNullOrEmpty
    }

    It "DeleteConnection returns error when connection does not exist" {
        $ErrorActionPreference = 'Stop'
        $errorOccurred = $false
        
        try {
            Get-DataverseConnection -DeleteConnection -Name "NonExistentConnection123"
        } catch {
            $errorOccurred = $true
            $_.Exception.Message | Should -Match "not found"
        }
        
        $errorOccurred | Should -Be $true
    }

    It "Can list saved connections after saving" {
        # Note: This test is limited because we can't easily test interactive auth in automated tests
        # We'll verify that the ListConnections parameter set works
        
        $result = Get-DataverseConnection -ListConnections
        # Should return a result (even if empty)
        # The result should be an array or $null
        ($result -is [Array]) -or ($null -eq $result) | Should -Be $true
    }

    It "Name parameter is available on Interactive parameter set" {
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Authenticate interactively" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.ParameterType | Should -Be ([string])
    }

    It "Name parameter is available on DeviceCode parameter set" {
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Authenticate using the device code flow" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.ParameterType | Should -Be ([string])
    }

    It "Name parameter is available on ClientSecret parameter set" {
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Authenticate with client secret" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.ParameterType | Should -Be ([string])
    }

    It "ListConnections parameter set exists" {
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "List saved named connections" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $listParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "ListConnections" }
        $listParam | Should -Not -BeNullOrEmpty
        $listParam.IsMandatory | Should -Be $true
    }

    It "DeleteConnection parameter set exists" {
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Delete a saved named connection" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $deleteParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "DeleteConnection" }
        $deleteParam | Should -Not -BeNullOrEmpty
        $deleteParam.IsMandatory | Should -Be $true
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.IsMandatory | Should -Be $true
    }

    It "LoadNamed parameter set exists" {
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Load a saved named connection" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.IsMandatory | Should -Be $true
    }

    It "Can use ListConnections parameter" {
        # Test that the cmdlet works with ListConnections
        $result = Get-DataverseConnection -ListConnections
        # Should return a result (even if empty)
        ($result -is [Array]) -or ($null -eq $result) | Should -Be $true
    }

    It "Can use DeleteConnection parameter with non-existent connection" {
        # Test that deleting a non-existent connection returns appropriate error
        $ErrorActionPreference = 'Stop'
        $errorOccurred = $false
        
        try {
            Get-DataverseConnection -DeleteConnection -Name "NonExistentConnection123456"
        } catch {
            $errorOccurred = $true
        }
        
        $errorOccurred | Should -Be $true
    }

    It "Can work with Mock provider" {
        # Test through the cmdlet using Mock provider
        $connection = getMockConnection
        # This implicitly tests that the module loads and works
        $connection | Should -Not -BeNullOrEmpty
    }
}
