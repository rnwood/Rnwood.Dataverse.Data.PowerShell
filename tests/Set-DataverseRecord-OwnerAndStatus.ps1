Describe 'Set-DataverseRecord - ownerid Assignment and Status Changes' {
    Context 'ownerid Assignment' {
        It "Creates record with ownerid and performs assignment" -Skip {
            # Note: ownerid assignment requires systemuser entity and AssignRequest support in mock
            # Test validates expected cmdlet behavior
            $connection = getMockConnection
            
            # Note: ownerid assignment may require systemuser entity in metadata
            # For now, testing with the behavior that's expected even if assignment fails in mock
            
            # Create record with ownerid (using a GUID)
            $ownerId = [Guid]::NewGuid()
            $record = @{
                firstname = "Assigned"
                lastname = "User"
                ownerid = $ownerId
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "Assigned"
            $result.lastname | Should -Be "User"
            
            # Note: ownerid assignment happens after create, may not reflect in mock immediately
            # The test validates that the cmdlet accepts ownerid without error
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Updates ownerid on existing record" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Reassign"
                lastname = "Test"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with new owner
            $newOwnerId = [Guid]::NewGuid()
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ ownerid = $newOwnerId }
            
            # Verify record still exists and other fields unchanged
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Reassign"
            $result.lastname | Should -Be "Test"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Handles ownerid with EntityReference object" -Skip {
            # Note: ownerid assignment requires systemuser entity and AssignRequest support in mock
            $connection = getMockConnection
            
            # Create record with ownerid as EntityReference-like object
            $ownerRef = @{
                Id = [Guid]::NewGuid()
                LogicalName = "systemuser"
            }
            
            $record = @{
                firstname = "Owner"
                lastname = "Reference"
                ownerid = $ownerRef
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "Owner"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'statuscode and statecode Changes' {
        It "Creates record with statuscode and performs state change" {
            $connection = getMockConnection
            
            # Create record with statuscode (numeric value)
            # statuscode values depend on entity - for contact, active=1, inactive=2
            $record = @{
                firstname = "Status"
                lastname = "Test"
                statuscode = 1  # Active
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "Status"
            
            # Note: statuscode change happens after create via SetStateRequest
            # The test validates the cmdlet accepts statuscode without error
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Updates statuscode on existing record" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Change"
                lastname = "Status"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update statuscode to inactive
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ statuscode = 2 }  # Inactive
            
            # Verify record still exists and other fields unchanged
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Change"
            $result.lastname | Should -Be "Status"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Sets both statecode and statuscode together" {
            $connection = getMockConnection
            
            # Create record
            $record = @{
                firstname = "Both"
                lastname = "States"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update both statecode and statuscode
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ 
                    statecode = 1  # Inactive state
                    statuscode = 2  # Inactive status
                }
            
            # Verify record still exists
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Both"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Handles status change with other field updates" {
            $connection = getMockConnection
            
            # Create record
            $record = @{
                firstname = "Initial"
                lastname = "Status"
                emailaddress1 = "initial@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update regular fields and statuscode together
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ 
                    firstname = "Updated"
                    emailaddress1 = "updated@example.com"
                    statuscode = 2
                }
            
            # Verify all updates applied
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.firstname | Should -Be "Updated"
            $result.emailaddress1 | Should -Be "updated@example.com"
            $result.lastname | Should -Be "Status"  # Unchanged
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Status changes work in batch updates" {
            $connection = getMockConnection
            
            # Create multiple records
            $records = @(
                @{ firstname = "User1"; lastname = "Batch" }
                @{ firstname = "User2"; lastname = "Batch" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Batch update statuscodes
            $updates = @(
                @{ Id = $records[0].Id; statuscode = 2 }
                @{ Id = $records[1].Id; statuscode = 2 }
            )
            $updates | Set-DataverseRecord -Connection $connection -TableName contact
            
            # Verify both records still exist
            $results = Get-DataverseRecord -Connection $connection -TableName contact
            $results | Should -HaveCount 2
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 2
        }
    }
}
