. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseRecord - MatchOn' {
    Context "MatchOn Support" {
        It "Retrieves a single record using MatchOn with single column" {
            $connection = getMockConnection
            
            # Create test records
            $record1 = @{ firstname = "John"; lastname = "Doe"; emailaddress1 = "john@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record2 = @{ firstname = "Jane"; lastname = "Smith"; emailaddress1 = "jane@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Retrieve using MatchOn
            $retrieved = @{ emailaddress1 = "john@test.com" } | 
                Get-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1
            
            $retrieved | Should -Not -BeNullOrEmpty
            $retrieved.firstname | Should -Be "John"
            $retrieved.emailaddress1 | Should -Be "john@test.com"
        }

        It "Retrieves a single record using MatchOn with multiple columns" {
            $connection = getMockConnection
            
            # Create test records with distinct combinations
            $record1 = @{ firstname = "Alice"; lastname = "Brown"; emailaddress1 = "alice.brown@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record2 = @{ firstname = "Bob"; lastname = "Green"; emailaddress1 = "bob.green@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Retrieve using MatchOn with multiple columns
            $retrieved = @{ firstname = "Alice"; lastname = "Brown" } | 
                Get-DataverseRecord -Connection $connection -TableName contact -MatchOn @("firstname", "lastname")
            
            $retrieved | Should -Not -BeNullOrEmpty
            $retrieved.firstname | Should -Be "Alice"
            $retrieved.lastname | Should -Be "Brown"
            $retrieved.emailaddress1 | Should -Be "alice.brown@test.com"
        }

        It "Raises error when MatchOn finds multiple matches without AllowMultipleMatches" {
            $connection = getMockConnection
            
            # Create multiple records with same email
            $record1 = @{ firstname = "John1"; lastname = "Doe"; emailaddress1 = "test@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record2 = @{ firstname = "John2"; lastname = "Doe"; emailaddress1 = "test@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Try to retrieve without AllowMultipleMatches - should error
            {
                @{ emailaddress1 = "test@test.com" } | 
                    Get-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -ErrorAction Stop
            } | Should -Throw "*AllowMultipleMatches*"
        }

        It "Retrieves all matching records with AllowMultipleMatches switch" {
            $connection = getMockConnection
            
            # Create multiple records with same last name
            $record1 = @{ firstname = "John"; lastname = "TestUser"; emailaddress1 = "john@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record2 = @{ firstname = "Jane"; lastname = "TestUser"; emailaddress1 = "jane@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record3 = @{ firstname = "Bob"; lastname = "Different"; emailaddress1 = "bob@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Retrieve all matching records
            $retrieved = @{ lastname = "TestUser" } | 
                Get-DataverseRecord -Connection $connection -TableName contact -MatchOn lastname -AllowMultipleMatches
            
            $retrieved | Should -HaveCount 2
            $retrieved.lastname | ForEach-Object { $_ | Should -Be "TestUser" }
        }

        It "Returns no records when MatchOn finds no matches" {
            $connection = getMockConnection
            
            # Try to retrieve non-existent record
            $retrieved = @{ emailaddress1 = "nonexistent@test.com" } | 
                Get-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1
            
            $retrieved | Should -BeNullOrEmpty
        }
    }
}
