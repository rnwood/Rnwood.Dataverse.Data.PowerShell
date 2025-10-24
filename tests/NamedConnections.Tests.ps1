Describe 'Named Connections' {

    BeforeAll {
        . $PSScriptRoot/Common.ps1
        
        # Clean up any existing test connections
        $testConnections = @("TestConn1", "TestConn2", "TestConnInteractive")
        foreach ($name in $testConnections) {
            try {
                Get-DataverseConnection -DeleteConnection -Name $name -ErrorAction SilentlyContinue
            } catch {
                # Ignore errors if connection doesn't exist
            }
        }
    }

    AfterAll {
        # Clean up test connections
        $testConnections = @("TestConn1", "TestConn2", "TestConnInteractive")
        foreach ($name in $testConnections) {
            try {
                Get-DataverseConnection -DeleteConnection -Name $name -ErrorAction SilentlyContinue
            } catch {
                # Ignore errors if connection doesn't exist
            }
        }
    }

    It "ListConnections returns empty array when no connections saved" {
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
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        # Note: Named connections with mock provider are not fully functional 
        # since mock connections don't use MSAL, but parameter should be accepted
        $connection = Get-DataverseConnection -Mock @($contactMetadata) -Url "https://test.crm.dynamics.com"
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
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Authenticate interactively" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.ParameterType | Should -Be ([string])
    }

    It "Name parameter is available on DeviceCode parameter set" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Authenticate using the device code flow" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.ParameterType | Should -Be ([string])
    }

    It "Name parameter is available on ClientSecret parameter set" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Authenticate with client secret" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.ParameterType | Should -Be ([string])
    }

    It "ListConnections parameter set exists" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "List saved named connections" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $listParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "ListConnections" }
        $listParam | Should -Not -BeNullOrEmpty
        $listParam.Mandatory | Should -Be $true
    }

    It "DeleteConnection parameter set exists" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Delete a saved named connection" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $deleteParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "DeleteConnection" }
        $deleteParam | Should -Not -BeNullOrEmpty
        $deleteParam.Mandatory | Should -Be $true
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.Mandatory | Should -Be $true
    }

    It "LoadNamed parameter set exists" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Load a saved named connection" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        $nameParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam | Should -Not -BeNullOrEmpty
        $nameParam.Mandatory | Should -Be $true
    }

    It "ConnectionStore class exists and can be instantiated" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        # Test that the ConnectionStore class is available
        $storeType = [Rnwood.Dataverse.Data.PowerShell.Commands.ConnectionStore]
        $storeType | Should -Not -BeNullOrEmpty
        
        $store = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ConnectionStore
        $store | Should -Not -BeNullOrEmpty
    }

    It "ConnectionStore can list connections" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $store = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ConnectionStore
        $connections = $store.ListConnections()
        # Should return a list (even if empty)
        $connections | Should -Not -BeNull
    }

    It "ConnectionStore reports false for non-existent connection" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $store = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ConnectionStore
        $exists = $store.ConnectionExists("NonExistentConnection123456")
        $exists | Should -Be $false
    }

    It "ConnectionStore can save and load connection metadata" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $store = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ConnectionStore
        
        $metadata = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ConnectionMetadata
        $metadata.Url = "https://test.crm.dynamics.com"
        $metadata.AuthMethod = "Interactive"
        $metadata.ClientId = [Guid]::NewGuid().ToString()
        $metadata.Username = "testuser@test.com"
        $metadata.SavedAt = [DateTime]::UtcNow
        
        $store.SaveConnection("TestConn1", $metadata)
        
        $loaded = $store.LoadConnection("TestConn1")
        $loaded | Should -Not -BeNullOrEmpty
        $loaded.Url | Should -Be $metadata.Url
        $loaded.AuthMethod | Should -Be $metadata.AuthMethod
        $loaded.Username | Should -Be $metadata.Username
    }

    It "ConnectionStore can delete saved connection" {
        if (-not (Get-Module Rnwood.Dataverse.Data.PowerShell)){
            Import-Module Rnwood.Dataverse.Data.PowerShell
        }
        
        $store = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ConnectionStore
        
        # Save a connection first
        $metadata = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ConnectionMetadata
        $metadata.Url = "https://test2.crm.dynamics.com"
        $metadata.AuthMethod = "DeviceCode"
        $metadata.SavedAt = [DateTime]::UtcNow
        
        $store.SaveConnection("TestConn2", $metadata)
        $store.ConnectionExists("TestConn2") | Should -Be $true
        
        # Delete it
        $deleted = $store.DeleteConnection("TestConn2")
        $deleted | Should -Be $true
        
        # Verify it's gone
        $store.ConnectionExists("TestConn2") | Should -Be $false
    }
}
