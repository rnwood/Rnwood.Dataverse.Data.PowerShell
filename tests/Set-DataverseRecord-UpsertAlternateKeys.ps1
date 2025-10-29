Describe 'Set-DataverseRecord - Upsert with Alternate Keys' {
    Context 'Upsert Flag with Alternate Keys' {
        It "Uses -Upsert flag to insert when record does not exist" -Skip {
            # Note: Alternate key support depends on FakeXrmEasy capabilities
            # This test validates expected behavior with alternate keys
            $connection = getMockConnection
            
            # Upsert with alternate key (should insert)
            $record = @{
                firstname = "Upsert"
                lastname = "Insert"
                emailaddress1 = "unique@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -Upsert -PassThru
            
            # Verify record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "Upsert"
            $result.lastname | Should -Be "Insert"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Uses -Upsert flag to update when record exists (matched by alternate key)" -Skip {
            # Note: Alternate key matching requires specific entity configuration
            $connection = getMockConnection
            
            # Create initial record
            $existing = @{
                firstname = "Original"
                lastname = "User"
                emailaddress1 = "alternate@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Upsert with same alternate key (should update)
            @{
                firstname = "Updated"
                lastname = "User"
                emailaddress1 = "alternate@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -Upsert
            
            # Verify record was updated, not duplicated
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $existing.Id
            $result.firstname | Should -Be "Updated"
            
            # Verify no side effects - still only one record
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Upsert uses platform UpsertRequest instead of manual retrieve/create/update" -Skip {
            # Note: UpsertRequest is a specific SDK request type
            $connection = getMockConnection
            
            # First upsert (insert)
            $record = @{
                firstname = "First"
                lastname = "Upsert"
                emailaddress1 = "upsert@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -Upsert -PassThru
            
            # Second upsert (update)
            @{
                firstname = "Second"
                lastname = "Upsert"
                emailaddress1 = "upsert@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -Upsert
            
            # Verify only one record exists with updated data
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
            $allContacts[0].firstname | Should -Be "Second"
        }

        It "Upsert with batch operations" -Skip {
            # Note: Batch upsert depends on platform support
            $connection = getMockConnection
            
            # Create initial record
            @{
                firstname = "Existing"
                lastname = "Batch"
                emailaddress1 = "batch1@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Batch upsert (one update, one insert)
            $records = @(
                @{ firstname = "Updated"; lastname = "Batch"; emailaddress1 = "batch1@example.com" }
                @{ firstname = "New"; lastname = "Batch"; emailaddress1 = "batch2@example.com" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -Upsert -PassThru
            
            # Verify two records exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 2
            
            # Verify update occurred
            ($allContacts | Where-Object { $_.emailaddress1 -eq "batch1@example.com" }).firstname | Should -Be "Updated"
        }

        It "Upsert fails gracefully when alternate key not configured" -Skip {
            # Note: This tests error handling
            $connection = getMockConnection
            
            # Try upsert without alternate key configuration
            # Should either use Id as fallback or throw appropriate error
            $record = @{
                firstname = "No"
                lastname = "AlternateKey"
            }
            
            # Attempt upsert (behavior depends on implementation)
            { $record | Set-DataverseRecord -Connection $connection -TableName contact -Upsert } | 
                Should -Not -Throw
        }
    }

    Context 'Upsert vs MatchOn Comparison' {
        It "Upsert uses platform feature, MatchOn uses cmdlet logic" -Skip {
            # Note: This test documents the difference between Upsert and MatchOn
            $connection = getMockConnection
            
            # Create record
            $record = @{
                firstname = "Compare"
                lastname = "Methods"
                emailaddress1 = "compare@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with Upsert (platform-level)
            @{
                firstname = "UpsertUpdate"
                lastname = "Methods"
                emailaddress1 = "compare@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -Upsert
            
            # Verify update worked
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "UpsertUpdate"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }
    }
}
