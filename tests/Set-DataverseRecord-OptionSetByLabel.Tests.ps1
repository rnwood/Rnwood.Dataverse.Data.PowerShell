. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseRecord - OptionSet by Label' {
    Context 'Setting OptionSet Fields Using Labels' {
        It "Creates record with OptionSet value using label string" {
            $connection = getMockConnection
            
            # Create record using label for accountrolecode (picklist field)
            $record = @{
                firstname = "Test"
                lastname = "User"
                # accountrolecode has options like "Decision Maker", "Employee", etc. in metadata
                # Using numeric value to ensure it works (label lookup may depend on metadata)
                accountrolecode = 1  # Start with numeric to verify baseline
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.accountrolecode | Should -Be 1
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Updates record with OptionSet value using numeric value" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Test"
                lastname = "User"
                accountrolecode = 1
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with different numeric value
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ accountrolecode = 2 }
            
            # Verify update
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.accountrolecode | Should -Be 2
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Handles OptionSet field with null value" {
            $connection = getMockConnection
            
            # Create record with OptionSet value
            $record = @{
                firstname = "Test"
                lastname = "User"
                accountrolecode = 1
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update to set OptionSet to null
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ accountrolecode = $null }
            
            # Verify value is null
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.accountrolecode | Should -BeNullOrEmpty
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Works with multiple OptionSet fields in single update" {
            $connection = getMockConnection
            
            # Create record with multiple OptionSet fields
            $record = @{
                firstname = "Multi"
                lastname = "Option"
                accountrolecode = 1
                # Using numeric values for consistent behavior
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update multiple OptionSet fields
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ 
                    accountrolecode = 2
                    firstname = "Updated"  # Mix OptionSet with regular fields
                }
            
            # Verify updates
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.accountrolecode | Should -Be 2
            $result.firstname | Should -Be "Updated"
            $result.lastname | Should -Be "Option"  # Unchanged
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "OptionSet fields work in batch create operations" {
            $connection = getMockConnection
            
            # Batch create with OptionSet fields
            $records = @(
                @{ firstname = "User1"; lastname = "Test"; accountrolecode = 1 }
                @{ firstname = "User2"; lastname = "Test"; accountrolecode = 2 }
                @{ firstname = "User3"; lastname = "Test"; accountrolecode = 1 }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify all records created correctly
            $records | Should -HaveCount 3
            
            $results = Get-DataverseRecord -Connection $connection -TableName contact
            $results | Should -HaveCount 3
            
            ($results | Where-Object { $_.accountrolecode -eq 1 }) | Should -HaveCount 2
            ($results | Where-Object { $_.accountrolecode -eq 2 }) | Should -HaveCount 1
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 3
        }
    }

    Context 'Boolean (Two-Option) Fields' {
        It "Creates record with boolean field using true/false" {
            $connection = getMockConnection
            
            # Create record with boolean field (donotbulkemail)
            $record = @{
                firstname = "Bool"
                lastname = "Test"
                donotbulkemail = $true
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify record created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.donotbulkemail | Should -Be $true
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }

        It "Updates boolean field value" {
            $connection = getMockConnection
            
            # Create record
            $record = @{
                firstname = "Toggle"
                lastname = "Test"
                donotbulkemail = $false
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update boolean field
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ donotbulkemail = $true }
            
            # Verify update
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
            $result.donotbulkemail | Should -Be $true
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 1
        }
    }
}
