. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseRecord - NoUpdate and NoCreate Flags' {
    Context 'NoUpdate Flag' {
        It "With -NoUpdate, creates new records but does not update existing ones" {
            $connection = getMockConnection
            
            # Create initial record
            $existing = @{
                firstname = "Original"
                lastname = "User"
                emailaddress1 = "original@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Try to update with NoUpdate (should not update)
            $updateAttempt = @{
                Id = $existing.Id
                firstname = "Updated"
                emailaddress1 = "updated@example.com"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $updateAttempt -NoUpdate
            
            # Verify record was NOT updated
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $existing.Id
            $result.firstname | Should -Be "Original"
            $result.emailaddress1 | Should -Be "original@example.com"
            
            # Verify no side effects - only one record
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "With -NoUpdate, creates new records when they don't exist" {
            $connection = getMockConnection
            
            # Create new record with NoUpdate
            $newRecord = @{
                firstname = "New"
                lastname = "Record"
                emailaddress1 = "new@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -NoUpdate -PassThru
            
            # Verify record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $newRecord.Id
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "New"
            $result.lastname | Should -Be "Record"
            $result.emailaddress1 | Should -Be "new@example.com"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "With -NoUpdate and -MatchOn, creates when no match found" {
            $connection = getMockConnection
            
            # Create existing record
            @{
                firstname = "Existing"
                lastname = "Different"
                emailaddress1 = "existing@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Try to create with MatchOn and NoUpdate
            $newRecord = @{
                firstname = "New"
                lastname = "User"
                emailaddress1 = "unique@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -NoUpdate -PassThru
            
            # Verify new record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $newRecord.Id
            $result.firstname | Should -Be "New"
            $result.emailaddress1 | Should -Be "unique@example.com"
            
            # Verify both records exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 2
        }

        It "With -NoUpdate and -MatchOn, does not update when match found" {
            $connection = getMockConnection
            
            # Create existing record
            $existing = @{
                firstname = "Original"
                lastname = "User"
                emailaddress1 = "match@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Try to update with MatchOn and NoUpdate (should not update)
            @{
                firstname = "Updated"
                lastname = "User"
                emailaddress1 = "match@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -NoUpdate
            
            # Verify record was NOT updated
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $existing.Id
            $result.firstname | Should -Be "Original"
            
            # Verify no side effects - still one record
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'NoCreate Flag' {
        It "With -NoCreate, updates existing records but does not create new ones" {
            $connection = getMockConnection
            
            # Create initial record
            $existing = @{
                firstname = "Original"
                lastname = "User"
                emailaddress1 = "test@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with NoCreate (should update)
            $updateData = @{
                Id = $existing.Id
                firstname = "Updated"
                emailaddress1 = "updated@example.com"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $updateData -NoCreate
            
            # Verify record WAS updated
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $existing.Id
            $result.firstname | Should -Be "Updated"
            $result.emailaddress1 | Should -Be "updated@example.com"
            
            # Verify no side effects - still one record
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "With -NoCreate, does not create new records" {
            $connection = getMockConnection
            
            # Try to create with NoCreate (should not create)
            $newRecord = @{
                firstname = "New"
                lastname = "Record"
                emailaddress1 = "new@example.com"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $newRecord -NoCreate
            
            # Verify NO records were created
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -BeNullOrEmpty
        }

        It "With -NoCreate and -MatchOn, updates when match found" {
            $connection = getMockConnection
            
            # Create existing record
            $existing = @{
                firstname = "Original"
                lastname = "User"
                emailaddress1 = "match@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with MatchOn and NoCreate (should update)
            @{
                firstname = "Updated"
                lastname = "User"
                emailaddress1 = "match@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -NoCreate
            
            # Verify record WAS updated
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $existing.Id
            $result.firstname | Should -Be "Updated"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "With -NoCreate and -MatchOn, does not create when no match found" {
            $connection = getMockConnection
            
            # Create existing record
            @{
                firstname = "Existing"
                lastname = "User"
                emailaddress1 = "existing@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Try to create with MatchOn and NoCreate (should not create)
            @{
                firstname = "New"
                lastname = "User"
                emailaddress1 = "nomatch@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -NoCreate
            
            # Verify new record was NOT created
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
            $allContacts[0].emailaddress1 | Should -Be "existing@example.com"
        }
    }

    Context 'Combined Scenarios' {
        It "NoUpdate and NoCreate cannot be used together" -Skip {
            # Note: This test validates cmdlet parameter validation
            # May require ParameterSet validation in cmdlet implementation
            $connection = getMockConnection
            
            # Try to use both flags together (should error)
            $record = @{
                firstname = "Test"
                lastname = "User"
            }
            
            # This should throw an error or be invalid
            { Set-DataverseRecord -Connection $connection -TableName contact -InputObject $record -NoUpdate -NoCreate } | 
                Should -Throw
        }
    }
}
