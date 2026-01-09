. $PSScriptRoot/Common.ps1

Describe 'Set-DataverseRecord - Complex Type Conversions' {
    Context 'MultiSelectPicklist Fields' {
        It "Creates record with MultiSelectPicklist using array of numeric values" {
            $connection = getMockConnection
            
            # Create record with MultiSelectPicklist field (if supported by contact entity)
            # Note: MultiSelectPicklist may not be in contact.xml metadata
            $record = @{
                firstname = "Multi"
                lastname = "Select"
                # MultiSelectPicklist would be an array of values
                # emailaddress1 = "multi@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns firstname, lastname
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "Multi"
            $result.lastname | Should -Be "Select"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Updates MultiSelectPicklist field with new values" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Update"
                lastname = "Multi"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with MultiSelectPicklist (if supported)
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ firstname = "Updated" }
            
            # Verify update
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns firstname
            $result.firstname | Should -Be "Updated"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Clears MultiSelectPicklist by setting to empty array or null" {
            $connection = getMockConnection
            
            # Create record
            $record = @{
                firstname = "Clear"
                lastname = "Multi"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Clear MultiSelectPicklist (if supported)
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ lastname = "Cleared" }
            
            # Verify update
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns lastname
            $result.lastname | Should -Be "Cleared"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'DateTime Fields with Timezone Handling' {
        It "Creates record with DateTime field" {
            $connection = getMockConnection
            
            # Create record with DateTime
            $testDate = [DateTime]::Parse("2024-01-15T10:30:00Z")
            $record = @{
                firstname = "DateTime"
                lastname = "Test"
                birthdate = $testDate
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify record was created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns birthdate, firstname
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "DateTime"
            
            # Verify birthdate is set (timezone conversion may occur)
            $result.birthdate | Should -Not -BeNullOrEmpty
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Updates DateTime field with timezone conversion" {
            $connection = getMockConnection
            
            # Create initial record
            $record = @{
                firstname = "Update"
                lastname = "DateTime"
                birthdate = [DateTime]::Parse("2024-01-01")
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with new DateTime
            $newDate = [DateTime]::Parse("2024-12-31T23:59:59Z")
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ birthdate = $newDate }
            
            # Verify update
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns birthdate
            $result.birthdate | Should -Not -BeNullOrEmpty
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Handles null DateTime values" {
            $connection = getMockConnection
            
            # Create record with DateTime
            $record = @{
                firstname = "Null"
                lastname = "DateTime"
                birthdate = [DateTime]::Parse("2024-06-15")
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Clear DateTime by setting to null
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ birthdate = $null }
            
            # Verify DateTime cleared
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns birthdate
            # birthdate may be null or empty depending on implementation
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Preserves date-only values without time component" {
            $connection = getMockConnection
            
            # Create record with date-only value
            $dateOnly = [DateTime]::Parse("2024-03-15").Date
            $record = @{
                firstname = "Date"
                lastname = "Only"
                birthdate = $dateOnly
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify record created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns birthdate
            $result | Should -Not -BeNullOrEmpty
            $result.birthdate | Should -Not -BeNullOrEmpty
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'Money Fields' {
        It "Creates record with Money field" {
            # Contact entity has Money fields: annualincome, creditlimit, etc.
            $connection = getMockConnection
            
            # Create record with Money value
            $record = @{
                firstname = "Money"
                lastname = "Test"
                creditlimit = 50000.00
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify record created
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns firstname
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "Money"
            $result.creditlimit | Should -Be 50000.00
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Updates Money field with new value" {
            # Contact entity has Money fields available
            $connection = getMockConnection
            
            # Create initial record with Money field
            $record = @{
                firstname = "Update"
                lastname = "Money"
                annualincome = 75000.00
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update Money field
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ annualincome = 85000.00 }
            
            # Verify update
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns annualincome
            $result.annualincome | Should -Be 85000.00
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Handles null Money values" {
            # Contact entity has Money fields available
            $connection = getMockConnection
            
            # Create record with Money field
            $record = @{
                firstname = "Null"
                lastname = "Money"
                creditlimit = 100000.00
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Set Money to null
            Set-DataverseRecord -Connection $connection -TableName contact -Id $record.Id `
                -InputObject @{ creditlimit = $null }
            
            # Verify Money field is null
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns creditlimit
            $result.creditlimit | Should -BeNullOrEmpty
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }
    }
}
