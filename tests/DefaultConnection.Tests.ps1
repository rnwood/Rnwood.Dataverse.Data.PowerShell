Describe 'Default Connection' {

    . $PSScriptRoot/Common.ps1

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
}
