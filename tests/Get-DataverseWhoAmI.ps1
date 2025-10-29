Describe 'Get-DataverseWhoAmI' {
    Context 'Identity Information Retrieval' {
        It "Returns WhoAmI response with identity information" {
            $connection = getMockConnection
            
            # Execute WhoAmI
            $result = Get-DataverseWhoAmI -Connection $connection
            
            # Verify result structure
            $result | Should -Not -BeNullOrEmpty
            
            # WhoAmI should return UserId, BusinessUnitId, and OrganizationId
            # These are GUIDs that identify the current user and organization
            $result.UserId | Should -Not -BeNullOrEmpty
            $result.UserId | Should -BeOfType [Guid]
            
            $result.BusinessUnitId | Should -Not -BeNullOrEmpty
            $result.BusinessUnitId | Should -BeOfType [Guid]
            
            $result.OrganizationId | Should -Not -BeNullOrEmpty
            $result.OrganizationId | Should -BeOfType [Guid]
            
            # Verify no side effects (WhoAmI is read-only)
            # No data should be created or modified
        }

        It "Returns consistent identity information on multiple calls" {
            $connection = getMockConnection
            
            # Call WhoAmI twice
            $result1 = Get-DataverseWhoAmI -Connection $connection
            $result2 = Get-DataverseWhoAmI -Connection $connection
            
            # Verify results are consistent
            $result1.UserId | Should -Be $result2.UserId
            $result1.BusinessUnitId | Should -Be $result2.BusinessUnitId
            $result1.OrganizationId | Should -Be $result2.OrganizationId
            
            # Verify no side effects
        }

        It "Works with default connection" {
            $connection = getMockConnection
            
            # Set as default
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $result = Get-DataverseWhoAmI
            
            # Verify result
            $result | Should -Not -BeNullOrEmpty
            $result.UserId | Should -BeOfType [Guid]
            
            # Verify no side effects
        }

        It "Does not modify any data (read-only operation)" {
            $connection = getMockConnection
            
            # Create test record to verify no side effects
            $contact = @{
                firstname = "Test"
                lastname = "WhoAmI"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Execute WhoAmI
            $result = Get-DataverseWhoAmI -Connection $connection
            
            # Verify contact was not affected
            $verifyContact = Get-DataverseRecord -Connection $connection -TableName contact -Id $contact.Id
            $verifyContact.firstname | Should -Be "Test"
            $verifyContact.lastname | Should -Be "WhoAmI"
            
            # Verify no side effects - still only one record
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }
    }
}
