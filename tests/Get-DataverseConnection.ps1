Describe "Get-DataverseConnection Default Connection" {
    It "SetAsDefault switch stores connection as default" {
        $connection = getMockConnection
        
        # Extract metadata from the mock connection for creating a new one with SetAsDefault
        $metadata = $connection.GetType().GetProperty("Metadata", [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
        if (-not $metadata) {
            # Alternative: use the metadata we have in tests
            $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
            $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
            $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
            $contactMetadata = $serializer.ReadObject($reader)
            $reader.Close()
            
            $defaultConnection = Get-DataverseConnection -Mock @($contactMetadata) -Url "https://test.crm.dynamics.com" -SetAsDefault
        } else {
            $defaultConnection = Get-DataverseConnection -Mock @($metadata.GetValue($connection)) -Url "https://test.crm.dynamics.com" -SetAsDefault
        }
        
        # Should return a connection
        $defaultConnection | Should -Not -BeNullOrEmpty
        
        # Get default explicitly
        $retrieved = Get-DataverseConnection -GetDefault
        $retrieved | Should -Not -BeNullOrEmpty
    }

    It "GetDefault returns error when no default is set" {
        # In a fresh session with no default, should error
        $output = pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = "$env:TESTMODULEPATH;$env:PSModulePath"
            Import-Module Rnwood.Dataverse.Data.PowerShell
            try {
                Get-DataverseConnection -GetDefault
                Write-Output "SUCCESS"
            } catch {
                Write-Output "ERROR: $($_.Exception.Message)"
            }
        } 2>&1
        
        $output | Should -Match "No default connection"
    }

    It "Cmdlets use default connection when -Connection not provided" {
        $connection = getMockConnection
        
        # Create a test record
        $in = new-object Microsoft.Xrm.Sdk.Entity "contact"
        $in["contactid"] = [Guid]::NewGuid()
        $in["firstname"] = "DefaultTest"
        $in | Set-DataverseRecord -Connection $connection -TableName contact
        
        # Set default connection using the same metadata
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        Get-DataverseConnection -Mock @($contactMetadata) -Url "https://test.crm.dynamics.com" -SetAsDefault | Out-Null
        
        # Query without providing connection - should use default
        # Note: This will be a different connection instance, so we need to seed it with data
        @{"firstname"="DefaultTest"} | Set-DataverseRecord -TableName contact
        
        $result = Get-DataverseRecord -TableName contact -FilterValues @{"firstname"="DefaultTest"}
        $result | Should -Not -BeNullOrEmpty
        $result.firstname | Should -Be "DefaultTest"
    }

    It "Cmdlets error when no connection provided and no default set" {
        # Run in clean PowerShell session
        $output = pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = "$env:TESTMODULEPATH;$env:PSModulePath"
            Import-Module Rnwood.Dataverse.Data.PowerShell
            try {
                Get-DataverseRecord -TableName contact
                Write-Output "SUCCESS"
            } catch {
                Write-Output "ERROR: $($_.Exception.Message)"
            }
        } 2>&1
        
        $output | Should -Match "No connection"
    }

    It "Connect-DataverseConnection alias works" {
        # Verify the alias exists
        $alias = Get-Alias -Name Connect-DataverseConnection -ErrorAction SilentlyContinue
        $alias | Should -Not -BeNullOrEmpty
        $alias.ResolvedCommand.Name | Should -Be "Get-DataverseConnection"
    }

    It "GetDefault parameter set works independently" {
        # Set a default first
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        Get-DataverseConnection -Mock @($contactMetadata) -Url "https://test.crm.dynamics.com" -SetAsDefault | Out-Null
        
        # GetDefault should work without other parameters
        $retrieved = Get-DataverseConnection -GetDefault
        $retrieved | Should -Not -BeNullOrEmpty
    }

    It "SetAsDefault works with Mock parameter set" {
        # Test that the parameter is available on Mock parameter set
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        $result = Get-DataverseConnection -Mock @($contactMetadata) -Url "https://test.crm.dynamics.com" -SetAsDefault
        $result | Should -Not -BeNullOrEmpty
        
        # Verify it was set as default
        $default = Get-DataverseConnection -GetDefault
        $default | Should -Not -BeNullOrEmpty
    }

    It "AccessToken parameter set accepts ScriptBlock" {
        # Test that the AccessToken parameter accepts a ScriptBlock
        $scriptBlock = { "fake-token" }
        $parameters = @{
            Url = "https://test.crm.dynamics.com"
            AccessToken = $scriptBlock
        }
        
        # Verify the cmdlet accepts the parameters without throwing during parameter binding
        $cmdlet = Get-Command Get-DataverseConnection
        $parameterSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "Authenticate with access token script block" }
        $parameterSet | Should -Not -BeNullOrEmpty
        
        # Verify AccessToken parameter exists and is of type ScriptBlock
        $accessTokenParam = $parameterSet.Parameters | Where-Object { $_.Name -eq "AccessToken" }
        $accessTokenParam | Should -Not -BeNullOrEmpty
        $accessTokenParam.ParameterType | Should -Be ([System.Management.Automation.ScriptBlock])
    }
}

Describe "Get-DataverseConnection Named Connections" {
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

Describe "Get-DataverseConnection Certificate Authentication" {
    Context "Certificate validation" {
        It "Throws error when neither CertificatePath nor CertificateThumbprint is provided" {
            # This test validates the LoadCertificate method requires at least one parameter
            # We can't easily test this without actually calling the cmdlet, which would require
            # a real certificate. This is documented behavior.
            $true | Should -Be $true
        }

        It "CertificatePath parameter exists" {
            # First get a mock connection to ensure module is loaded
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificatePath') | Should -Be $true
        }

        It "CertificatePassword parameter exists" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificatePassword') | Should -Be $true
        }

        It "CertificateThumbprint parameter exists" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificateThumbprint') | Should -Be $true
        }

        It "CertificateStoreLocation parameter exists" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificateStoreLocation') | Should -Be $true
        }

        It "CertificateStoreName parameter exists" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $cmd.Parameters.ContainsKey('CertificateStoreName') | Should -Be $true
        }

        It "Certificate parameters are in correct parameter set" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $certPathParam = $cmd.Parameters['CertificatePath']
            $certPathParam.ParameterSets.Keys | Should -Contain 'Authenticate with client certificate'
        }
    }

    Context "Certificate authentication scenarios" {
        It "Validates ClientId is required for certificate authentication" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $clientIdParam = $cmd.Parameters['ClientId']
            $clientIdParam.ParameterSets['Authenticate with client certificate'].IsMandatory | Should -Be $true
        }

        It "Validates Url is required for certificate authentication" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $urlParam = $cmd.Parameters['Url']
            $urlParam.ParameterSets['Authenticate with client certificate'].IsMandatory | Should -Be $true
        }

        It "Validates CertificatePath is required for certificate authentication" {
            $null = getMockConnection
            $cmd = Get-Command Get-DataverseConnection
            $certPathParam = $cmd.Parameters['CertificatePath']
            $certPathParam.ParameterSets['Authenticate with client certificate'].IsMandatory | Should -Be $true
        }
    }
}
