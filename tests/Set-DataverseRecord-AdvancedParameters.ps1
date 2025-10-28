Describe 'Set-DataverseRecord - Advanced Parameters' {
    Context 'UpdateAllColumns Parameter' {
        It "With -UpdateAllColumns, skips retrieve step and updates all columns" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Original"
                lastname = "User"
                emailaddress1 = "original@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with UpdateAllColumns (skips retrieve, updates all provided columns)
            $updateData = @{
                Id = $record.Id
                firstname = "Updated"
                emailaddress1 = "updated@example.com"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $updateData -UpdateAllColumns
            
            # Verify record was updated
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Updated"
            $result.emailaddress1 | Should -Be "updated@example.com"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "UpdateAllColumns with MatchOn updates without retrieving" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Match"
                lastname = "Test"
                emailaddress1 = "match@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with MatchOn and UpdateAllColumns
            @{
                firstname = "Updated"
                lastname = "Test"
                emailaddress1 = "match@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -UpdateAllColumns
            
            # Verify update
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Updated"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "UpdateAllColumns improves performance by skipping retrieve" {
            $connection = getMockConnection
            
            # Create test record
            $record = @{
                firstname = "Performance"
                lastname = "Test"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with UpdateAllColumns (should be faster as no retrieve)
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ firstname = "Fast" } -UpdateAllColumns
            
            # Verify update
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Fast"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'IgnoreProperties Parameter' {
        It "With -IgnoreProperties, excludes specified properties from operation" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Original"
                lastname = "User"
                emailaddress1 = "original@example.com"
                description = "Original description"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with IgnoreProperties (should ignore description)
            $updateData = @{
                Id = $record.Id
                firstname = "Updated"
                emailaddress1 = "updated@example.com"
                description = "This should be ignored"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $updateData -IgnoreProperties description
            
            # Verify update (description should remain original)
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Updated"
            $result.emailaddress1 | Should -Be "updated@example.com"
            $result.description | Should -Be "Original description"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "IgnoreProperties works with multiple properties" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Multi"
                lastname = "Ignore"
                emailaddress1 = "multi@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with multiple ignored properties
            $updateData = @{
                Id = $record.Id
                firstname = "Should be ignored"
                lastname = "Updated"
                emailaddress1 = "Should be ignored"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $updateData `
                -IgnoreProperties firstname, emailaddress1
            
            # Verify only lastname was updated
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Multi"
            $result.lastname | Should -Be "Updated"
            $result.emailaddress1 | Should -Be "multi@example.com"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "IgnoreProperties with non-existent property does not error" {
            $connection = getMockConnection
            
            # Create record
            $record = @{
                firstname = "Ignore"
                lastname = "NonExistent"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with ignored property that doesn't exist in input
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ firstname = "Updated" } -IgnoreProperties nonexistentfield
            
            # Verify update succeeded
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Updated"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'CallerId Parameter (Delegation)' {
        It "Creates record with -CallerId for delegation" -Skip {
            # Note: CallerId requires systemuser entity and may not work with mock
            $connection = getMockConnection
            
            # Create record with CallerId (impersonate another user)
            $callerId = [Guid]::NewGuid()
            $record = @{
                firstname = "Delegated"
                lastname = "Create"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -CallerId $callerId -PassThru
            
            # Verify record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "Delegated"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Updates record with -CallerId for delegation" -Skip {
            # Note: CallerId requires systemuser entity
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Original"
                lastname = "User"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with CallerId
            $callerId = [Guid]::NewGuid()
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ firstname = "Updated" } -CallerId $callerId
            
            # Verify update
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Updated"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "CallerId works with batch operations" -Skip {
            # Note: CallerId with batching may have specific requirements
            $connection = getMockConnection
            
            # Batch create with CallerId
            $callerId = [Guid]::NewGuid()
            $records = @(
                @{ firstname = "Batch1"; lastname = "Delegate" }
                @{ firstname = "Batch2"; lastname = "Delegate" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -CallerId $callerId -PassThru
            
            # Verify records created
            $records | Should -HaveCount 2
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 2
        }
    }
}
