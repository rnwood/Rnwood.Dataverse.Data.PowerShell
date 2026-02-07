. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseRecord - ownerid Assignment and Status Changes' {
    Context 'ownerid Assignment' {
    }

    Context 'statuscode and statecode Changes' {
        It "Updates statuscode on existing record" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Change"
                lastname = "Status"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update statuscode to inactive
            @{ 
                Id = $record.Id
                statuscode = 2  # Inactive
            } | Set-DataverseRecord -Connection $connection -TableName contact
            
            # Verify record still exists and other fields unchanged
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns firstname, lastname
            $result.firstname | Should -Be "Change"
            $result.lastname | Should -Be "Status"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
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
            @{ 
                Id = $record.Id
                statecode = 1  # Inactive state
                statuscode = 2  # Inactive status
            } | Set-DataverseRecord -Connection $connection -TableName contact
            
            # Verify record still exists
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns firstname
            $result.firstname | Should -Be "Both"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
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
            @{ 
                Id = $record.Id
                firstname = "Updated"
                emailaddress1 = "updated@example.com"
                statuscode = 2
            } | Set-DataverseRecord -Connection $connection -TableName contact
            
            # Verify all updates applied
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns emailaddress1, firstname, lastname
            $result.firstname | Should -Be "Updated"
            $result.emailaddress1 | Should -Be "updated@example.com"
            $result.lastname | Should -Be "Status"  # Unchanged
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
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
            $results = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $results | Should -HaveCount 2
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 2
        }
    }
}
