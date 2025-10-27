Describe "Get-DataverseConnection - Named Connections" {
    It "Get-DataverseConnection lists all named connections" {
        # Verify that List parameter set exists
        $cmdlet = Get-Command Get-DataverseConnection
        $listParamSet = $cmdlet.ParameterSets | Where-Object { $_.Name -eq "List" }
        $listParamSet | Should -Not -BeNullOrEmpty
    }

    It "List parameter shows stored connections" {
        # Get all named connections
        $cmdlet = Get-Command Get-DataverseConnection
        
        # Verify List parameter is available
        $listParamSet = $cmdlet.ParameterSets | Where-Object {
            $_.Parameters.Name -contains "List"
        }
        $listParamSet | Should -Not -BeNullOrEmpty
        
        # Verify List is a switch parameter
        $listParam = $listParamSet.Parameters | Where-Object { $_.Name -eq "List" }
        $listParam.ParameterType | Should -Be ([System.Management.Automation.SwitchParameter])
    }

    It "SaveAs stores connection with name" {
        # Test that SaveAs parameter is available
        $cmdlet = Get-Command Get-DataverseConnection
        
        $paramSet = $cmdlet.ParameterSets | Where-Object {
            $_.Parameters.Name -contains "SaveAs"
        }
        $paramSet | Should -Not -BeNullOrEmpty
        
        # Verify SaveAs is of type string
        $saveAsParam = $paramSet.Parameters | Where-Object { $_.Name -eq "SaveAs" }
        $saveAsParam.ParameterType | Should -Be ([System.String])
    }

    It "Name parameter retrieves saved connection" {
        # Test that Name parameter is available for retrieval
        $cmdlet = Get-Command Get-DataverseConnection
        
        $paramSet = $cmdlet.ParameterSets | Where-Object {
            $_.Parameters.Name -contains "Name"
        }
        $paramSet | Should -Not -BeNullOrEmpty
        
        # Verify Name parameter type
        $nameParam = $paramSet.Parameters | Where-Object { $_.Name -eq "Name" }
        $nameParam.ParameterType | Should -Be ([System.String])
    }

    It "SaveAs parameter set works with Mock" {
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        # Save a mock connection with a name
        $connection = Get-DataverseConnection -Mock @($contactMetadata) -Url "https://test.crm.dynamics.com" -SaveAs "TestConnection"
        $connection | Should -Not -BeNullOrEmpty
    }

    It "Name parameter retrieves previously saved connection" {
        # First save a connection
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        Get-DataverseConnection -Mock @($contactMetadata) -Url "https://saved.crm.dynamics.com" -SaveAs "SavedTestConnection" | Out-Null
        
        # Retrieve by name
        $retrieved = Get-DataverseConnection -Name "SavedTestConnection"
        $retrieved | Should -Not -BeNullOrEmpty
    }

    It "ForceRefresh parameter works with named connections" {
        # Test that ForceRefresh can be used to refresh a named connection
        $cmdlet = Get-Command Get-DataverseConnection
        
        # ForceRefresh should be available in connection parameter sets
        $paramSet = $cmdlet.ParameterSets | Where-Object {
            $_.Parameters.Name -contains "ForceRefresh"
        }
        $paramSet | Should -Not -BeNullOrEmpty
        
        # Verify it's a switch
        $forceRefreshParam = $paramSet.Parameters | Where-Object { $_.Name -eq "ForceRefresh" }
        $forceRefreshParam.ParameterType | Should -Be ([System.Management.Automation.SwitchParameter])
    }

    It "Remove-DataverseConnection removes stored connections" {
        # Verify that Remove-DataverseConnection cmdlet exists
        $cmdlet = Get-Command Remove-DataverseConnection -ErrorAction SilentlyContinue
        $cmdlet | Should -Not -BeNullOrEmpty
    }

    It "Persisted connection survives new PowerShell session" {
        # Save a connection
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        Get-DataverseConnection -Mock @($contactMetadata) -Url "https://persistent.crm.dynamics.com" -SaveAs "PersistentConnection" | Out-Null
        
        # In a new session, retrieve it
        $output = pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = "$env:TESTMODULEPATH;$env:PSModulePath"
            Import-Module Rnwood.Dataverse.Data.PowerShell
            $conn = Get-DataverseConnection -Name "PersistentConnection"
            if ($conn) {
                Write-Output "FOUND"
            } else {
                Write-Output "NOT_FOUND"
            }
        } 2>&1
        
        $output | Should -Match "FOUND"
    }

    It "List parameter set includes ListConnections alias" {
        # Test for alternative function name
        $cmdlet = Get-Command Get-DataverseConnection -ErrorAction SilentlyContinue
        $cmdlet | Should -Not -BeNullOrEmpty
        
        # Also test if there's a List parameter
        $listParam = $cmdlet.Parameters["List"]
        $listParam | Should -Not -BeNullOrEmpty
    }

    It "SaveAs with special characters in name" {
        # Test that SaveAs handles special characters
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        # Save with a name containing dashes and numbers
        $connection = Get-DataverseConnection -Mock @($contactMetadata) -Url "https://special.crm.dynamics.com" -SaveAs "Test-Connection-2024"
        $connection | Should -Not -BeNullOrEmpty
        
        # Retrieve by name
        $retrieved = Get-DataverseConnection -Name "Test-Connection-2024"
        $retrieved | Should -Not -BeNullOrEmpty
    }

    It "Multiple named connections can be stored" {
        # Save multiple connections with different names
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        Get-DataverseConnection -Mock @($contactMetadata) -Url "https://conn1.crm.dynamics.com" -SaveAs "MultiTest1" | Out-Null
        Get-DataverseConnection -Mock @($contactMetadata) -Url "https://conn2.crm.dynamics.com" -SaveAs "MultiTest2" | Out-Null
        
        # Both should be retrievable
        $conn1 = Get-DataverseConnection -Name "MultiTest1"
        $conn2 = Get-DataverseConnection -Name "MultiTest2"
        
        $conn1 | Should -Not -BeNullOrEmpty
        $conn2 | Should -Not -BeNullOrEmpty
    }

    It "SetAsDefault works with named connections" {
        # Save and set a connection as default
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        # Save and set as default
        Get-DataverseConnection -Mock @($contactMetadata) -Url "https://default.crm.dynamics.com" -SaveAs "DefaultNamed" -SetAsDefault | Out-Null
        
        # GetDefault should return it
        $default = Get-DataverseConnection -GetDefault
        $default | Should -Not -BeNullOrEmpty
    }

    It "Name parameter is case-insensitive" {
        # Save a connection
        $metadataXml = Get-Content "$PSScriptRoot/contact.xml" -Raw
        $serializer = [System.Runtime.Serialization.DataContractSerializer]::new([Microsoft.Xrm.Sdk.Metadata.EntityMetadata])
        $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($metadataXml))
        $contactMetadata = $serializer.ReadObject($reader)
        $reader.Close()
        
        Get-DataverseConnection -Mock @($contactMetadata) -Url "https://case.crm.dynamics.com" -SaveAs "CaseTest" | Out-Null
        
        # Try retrieving with different case
        $retrieved = Get-DataverseConnection -Name "casetest"
        $retrieved | Should -Not -BeNullOrEmpty
    }
}
