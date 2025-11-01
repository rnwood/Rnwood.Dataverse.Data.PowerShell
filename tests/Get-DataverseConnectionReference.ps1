Describe 'Get-DataverseConnectionReference' {
    It "Returns connection references with expected properties" {
        $connection = getMockConnection

        # Create a test connection reference
        $connRefId = [Guid]::NewGuid()
        $testConnRef = new-object Microsoft.Xrm.Sdk.Entity "connectionreference"
        $testConnRef["connectionreferenceid"] = $connRefId
        $testConnRef["connectionreferencelogicalname"] = "test_connref"
        $testConnRef["connectionreferencedisplayname"] = "Test Connection Reference"
        $testConnRef["connectionid"] = "test_connection_id"
        $testConnRef["connectorid"] = "12345678-1234-1234-1234-123456789012"
        $testConnRef["description"] = "Test description"

        $testConnRef | Set-DataverseRecord -Connection $connection -CreateOnly

        $result = Get-DataverseConnectionReference -Connection $connection

        $result | Should -Not -BeNullOrEmpty
        $result | Should -BeOfType [PSCustomObject]

        # Check that the expected properties are present
        $result.PSObject.Properties.Name | Should -Contain "ConnectionReferenceId"
        $result.PSObject.Properties.Name | Should -Contain "ConnectionReferenceLogicalName"
        $result.PSObject.Properties.Name | Should -Contain "ConnectionReferenceDisplayName"
        $result.PSObject.Properties.Name | Should -Contain "ConnectionId"
        $result.PSObject.Properties.Name | Should -Contain "ConnectorId"
        $result.PSObject.Properties.Name | Should -Contain "Description"

        # Check specific values
        $result.ConnectionReferenceId | Should -Be $connRefId
        $result.ConnectionReferenceLogicalName | Should -Be "test_connref"
        $result.ConnectionReferenceDisplayName | Should -Be "Test Connection Reference"
        $result.ConnectionId | Should -Be "test_connection_id"
        $result.ConnectorId | Should -Be "12345678-1234-1234-1234-123456789012"
        $result.Description | Should -Be "Test description"
    }

    It "Filters by ConnectionReferenceLogicalName" {
        $connection = getMockConnection

        # Create test connection references
        $connRef1 = new-object Microsoft.Xrm.Sdk.Entity "connectionreference"
        $connRef1["connectionreferenceid"] = [Guid]::NewGuid()
        $connRef1["connectionreferencelogicalname"] = "test_connref1"
        $connRef1["connectionreferencedisplayname"] = "Test Connection Reference 1"
        $connRef1["connectorid"] = "12345678-1234-1234-1234-123456789012"

        $connRef2 = new-object Microsoft.Xrm.Sdk.Entity "connectionreference"
        $connRef2["connectionreferenceid"] = [Guid]::NewGuid()
        $connRef2["connectionreferencelogicalname"] = "test_connref2"
        $connRef2["connectionreferencedisplayname"] = "Test Connection Reference 2"
        $connRef2["connectorid"] = "87654321-4321-4321-4321-210987654321"

        $connRef1 | Set-DataverseRecord -Connection $connection -CreateOnly
        $connRef2 | Set-DataverseRecord -Connection $connection -CreateOnly

        $result = Get-DataverseConnectionReference -Connection $connection -ConnectionReferenceLogicalName "test_connref1"

        $result | Should -Not -BeNullOrEmpty
        $result.ConnectionReferenceLogicalName | Should -Be "test_connref1"
        $result.ConnectorId | Should -Be "12345678-1234-1234-1234-123456789012"
    }

    It "Filters by ConnectorId" {
        $connection = getMockConnection

        # Create test connection references
        $connRef1 = new-object Microsoft.Xrm.Sdk.Entity "connectionreference"
        $connRef1["connectionreferenceid"] = [Guid]::NewGuid()
        $connRef1["connectionreferencelogicalname"] = "test_connref1"
        $connRef1["connectionreferencedisplayname"] = "Test Connection Reference 1"
        $connRef1["connectorid"] = "12345678-1234-1234-1234-123456789012"

        $connRef2 = new-object Microsoft.Xrm.Sdk.Entity "connectionreference"
        $connRef2["connectionreferenceid"] = [Guid]::NewGuid()
        $connRef2["connectionreferencelogicalname"] = "test_connref2"
        $connRef2["connectionreferencedisplayname"] = "Test Connection Reference 2"
        $connRef2["connectorid"] = "87654321-4321-4321-4321-210987654321"

        $connRef1 | Set-DataverseRecord -Connection $connection -CreateOnly
        $connRef2 | Set-DataverseRecord -Connection $connection -CreateOnly

        $result = Get-DataverseConnectionReference -Connection $connection -ConnectorId "12345678-1234-1234-1234-123456789012"

        $result | Should -Not -BeNullOrEmpty
        $result.ConnectionReferenceLogicalName | Should -Be "test_connref1"
        $result.ConnectorId | Should -Be "12345678-1234-1234-1234-123456789012"
    }

    It "Supports wildcard filtering for ConnectorId" {
        $connection = getMockConnection

        # Create test connection references
        $connRef1 = new-object Microsoft.Xrm.Sdk.Entity "connectionreference"
        $connRef1["connectionreferenceid"] = [Guid]::NewGuid()
        $connRef1["connectionreferencelogicalname"] = "test_connref1"
        $connRef1["connectionreferencedisplayname"] = "Test Connection Reference 1"
        $connRef1["connectorid"] = "12345678-1234-1234-1234-123456789012"

        $connRef2 = new-object Microsoft.Xrm.Sdk.Entity "connectionreference"
        $connRef2["connectionreferenceid"] = [Guid]::NewGuid()
        $connRef2["connectionreferencelogicalname"] = "test_connref2"
        $connRef2["connectionreferencedisplayname"] = "Test Connection Reference 2"
        $connRef2["connectorid"] = "87654321-4321-4321-4321-210987654321"

        $connRef1 | Set-DataverseRecord -Connection $connection -CreateOnly
        $connRef2 | Set-DataverseRecord -Connection $connection -CreateOnly

        $results = Get-DataverseConnectionReference -Connection $connection -ConnectorId "12345678*"

        $results | Should -Not -BeNullOrEmpty
        $results.Count | Should -Be 1
        $results[0].ConnectionReferenceLogicalName | Should -Be "test_connref1"
        $results[0].ConnectorId | Should -Be "12345678-1234-1234-1234-123456789012"
    }
}