. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseRecord - NoUpdateColumns' {
    Context 'Excluding Columns from Updates' {
        It "Updates record but excludes specified column from update" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Original"
                lastname = "User"
                emailaddress1 = "original@example.com"
                description = "Original description"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with NoUpdateColumns
            $updateData = @{
                firstname = "Updated"
                emailaddress1 = "updated@example.com"
                description = "This should be ignored"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject $updateData -NoUpdateColumns description
            
            # Verify record was updated correctly
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Updated"
            $result.emailaddress1 | Should -Be "updated@example.com"
            $result.description | Should -Be "Original description"  # Should remain unchanged
            $result.lastname | Should -Be "User"  # Should remain unchanged (not in update)
            
            # Verify no side effects - only one record exists
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Excludes multiple columns from update" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "John"
                lastname = "Doe"
                emailaddress1 = "john@example.com"
                mobilephone = "555-1234"
                telephone1 = "555-5678"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with multiple NoUpdateColumns
            $updateData = @{
                firstname = "Jane"
                lastname = "Smith"
                emailaddress1 = "jane@example.com"
                mobilephone = "999-0000"
                telephone1 = "888-0000"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject $updateData -NoUpdateColumns emailaddress1, mobilephone, telephone1
            
            # Verify only allowed columns were updated
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Jane"
            $result.lastname | Should -Be "Smith"
            $result.emailaddress1 | Should -Be "john@example.com"  # Unchanged
            $result.mobilephone | Should -Be "555-1234"  # Unchanged
            $result.telephone1 | Should -Be "555-5678"  # Unchanged
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "NoUpdateColumns works with batch updates" {
            $connection = getMockConnection
            
            # Create initial records
            $records = @(
                @{ firstname = "User1"; lastname = "Test"; emailaddress1 = "user1@example.com" }
                @{ firstname = "User2"; lastname = "Test"; emailaddress1 = "user2@example.com" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Batch update with NoUpdateColumns
            $updates = @(
                @{ Id = $records[0].Id; firstname = "UpdatedUser1"; emailaddress1 = "shouldnotchange1@example.com" }
                @{ Id = $records[1].Id; firstname = "UpdatedUser2"; emailaddress1 = "shouldnotchange2@example.com" }
            )
            $updates | Set-DataverseRecord -Connection $connection -TableName contact -NoUpdateColumns emailaddress1
            
            # Verify both records were updated correctly
            $results = Get-DataverseRecord -Connection $connection -TableName contact
            $results | Should -HaveCount 2
            
            $user1 = $results | Where-Object { $_.Id -eq $records[0].Id }
            $user1.firstname | Should -Be "UpdatedUser1"
            $user1.emailaddress1 | Should -Be "user1@example.com"  # Unchanged
            
            $user2 = $results | Where-Object { $_.Id -eq $records[1].Id }
            $user2.firstname | Should -Be "UpdatedUser2"
            $user2.emailaddress1 | Should -Be "user2@example.com"  # Unchanged
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 2
        }

        It "NoUpdateColumns has no effect on create operations" {
            $connection = getMockConnection
            
            # Create with NoUpdateColumns (should be ignored for creates)
            $newRecord = @{
                firstname = "New"
                lastname = "User"
                emailaddress1 = "new@example.com"
            }
            $result = Set-DataverseRecord -Connection $connection -TableName contact `
                -InputObject $newRecord -CreateOnly -NoUpdateColumns emailaddress1 -PassThru
            
            # Verify record was created with all fields (NoUpdateColumns ignored)
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "New"
            $result.lastname | Should -Be "User"
            $result.emailaddress1 | Should -Be "new@example.com"  # Should be set despite NoUpdateColumns
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Excludes column that is not in the input object (no error)" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Test"
                lastname = "User"
                emailaddress1 = "test@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with NoUpdateColumns for column not in input
            $updateData = @{ firstname = "Updated" }
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject $updateData -NoUpdateColumns emailaddress1, description  # description not in input
            
            # Verify update succeeded
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Updated"
            $result.emailaddress1 | Should -Be "test@example.com"  # Unchanged
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }
    }
}
