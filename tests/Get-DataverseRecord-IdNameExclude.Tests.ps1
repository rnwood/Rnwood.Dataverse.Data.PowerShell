. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseRecord - Id, Name, and ExcludeId Parameters' {
    Context 'Id Parameter' {
        It "Retrieves specific records by list of Ids" {
            $connection = getMockConnection
            
            # Create test records
            $records = @(
                @{ firstname = "User1"; lastname = "Test1" }
                @{ firstname = "User2"; lastname = "Test2" }
                @{ firstname = "User3"; lastname = "Test3" }
                @{ firstname = "User4"; lastname = "Test4" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Get specific records by Id
            $targetIds = @($records[0].Id, $records[2].Id)
            $results = Get-DataverseRecord -Connection $connection -TableName contact -Id $targetIds -Columns firstname, lastname
            
            # Assert correct records returned
            $results | Should -HaveCount 2
            $results[0].Id | Should -BeIn $targetIds
            $results[1].Id | Should -BeIn $targetIds
            ($results | Where-Object { $_.firstname -eq "User1" }) | Should -HaveCount 1
            ($results | Where-Object { $_.firstname -eq "User3" }) | Should -HaveCount 1
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 4
        }

        It "Retrieves single record by Id" {
            $connection = getMockConnection
            
            # Create test record
            $record = @{ firstname = "Single"; lastname = "Record" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Get record by single Id
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns firstname, lastname
            
            # Assert correct record returned
            $result | Should -Not -BeNullOrEmpty
            $result.Id | Should -Be $record.Id
            $result.firstname | Should -Be "Single"
            $result.lastname | Should -Be "Record"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 1
        }

        It "Returns empty when Id not found" {
            $connection = getMockConnection
            
            # Create one record
            @{ firstname = "Existing"; lastname = "Record" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Try to get non-existent record
            $nonExistentId = [Guid]::NewGuid()
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Id $nonExistentId -Columns contactid
            
            # Assert no results
            $result | Should -BeNullOrEmpty
            
            # Verify no side effects - existing record still there
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'Name Parameter' {
        It "Retrieves records by list of names (primary attribute)" -Skip {
            # Note: Name-based lookups require fullname computation which may not work with FakeXrmEasy
            # This test would work with real Dataverse where fullname is auto-computed
            $connection = getMockConnection
            
            # Create test records with unique fullnames
            @(
                @{ firstname = "John"; lastname = "Doe" }
                @{ firstname = "Jane"; lastname = "Smith" }
                @{ firstname = "Bob"; lastname = "Johnson" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Get records by name (fullname is auto-generated from firstname + lastname)
            $targetNames = @("John Doe", "Bob Johnson")
            $results = Get-DataverseRecord -Connection $connection -TableName contact -Name $targetNames -Columns firstname, lastname
            
            # Assert correct records returned
            $results | Should -HaveCount 2
            ($results | Where-Object { $_.firstname -eq "John" -and $_.lastname -eq "Doe" }) | Should -HaveCount 1
            ($results | Where-Object { $_.firstname -eq "Bob" -and $_.lastname -eq "Johnson" }) | Should -HaveCount 1
            ($results | Where-Object { $_.firstname -eq "Jane" }) | Should -BeNullOrEmpty
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 3
        }

        It "Retrieves single record by name" -Skip {
            # Note: Name-based lookups require fullname computation which may not work with FakeXrmEasy
            $connection = getMockConnection
            
            # Create test record
            @{ firstname = "Unique"; lastname = "Person" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Get record by name
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Name "Unique Person" -Columns firstname, lastname
            
            # Assert correct record returned
            $result | Should -Not -BeNullOrEmpty
            $result.firstname | Should -Be "Unique"
            $result.lastname | Should -Be "Person"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 1
        }

        It "Returns empty when name not found" {
            $connection = getMockConnection
            
            # Create one record
            @{ firstname = "Existing"; lastname = "User" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Try to get non-existent name
            $result = Get-DataverseRecord -Connection $connection -TableName contact -Name "NonExistent Name" -Columns contactid
            
            # Assert no results
            $result | Should -BeNullOrEmpty
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'ExcludeId Parameter' {
        It "Excludes specific records by list of Ids" {
            $connection = getMockConnection
            
            # Create test records
            $records = @(
                @{ firstname = "Keep1"; lastname = "Test" }
                @{ firstname = "Exclude1"; lastname = "Test" }
                @{ firstname = "Keep2"; lastname = "Test" }
                @{ firstname = "Exclude2"; lastname = "Test" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Exclude specific records
            $excludeIds = @($records[1].Id, $records[3].Id)  # Exclude1 and Exclude2
            $results = Get-DataverseRecord -Connection $connection -TableName contact -ExcludeId $excludeIds -Columns firstname, lastname
            
            # Assert correct records returned (only Keep1 and Keep2)
            $results | Should -HaveCount 2
            ($results | Where-Object { $_.firstname -eq "Keep1" }) | Should -HaveCount 1
            ($results | Where-Object { $_.firstname -eq "Keep2" }) | Should -HaveCount 1
            ($results | Where-Object { $_.firstname -like "Exclude*" }) | Should -BeNullOrEmpty
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 4
        }

        It "Works with filter and ExcludeId combined" {
            $connection = getMockConnection
            
            # Create test records
            $records = @(
                @{ firstname = "Alice"; lastname = "MatchFilter" }
                @{ firstname = "Bob"; lastname = "MatchFilter" }
                @{ firstname = "Charlie"; lastname = "MatchFilter" }
                @{ firstname = "David"; lastname = "NoMatch" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Get records with filter but exclude one
            $excludeId = $records[1].Id  # Exclude Bob
            $results = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname `
                -FilterValues @{ lastname = "MatchFilter" } `
                -ExcludeId $excludeId
            
            # Assert correct records returned (Alice and Charlie, but not Bob)
            $results | Should -HaveCount 2
            ($results | Where-Object { $_.firstname -eq "Alice" }) | Should -HaveCount 1
            ($results | Where-Object { $_.firstname -eq "Charlie" }) | Should -HaveCount 1
            ($results | Where-Object { $_.firstname -eq "Bob" }) | Should -BeNullOrEmpty
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 4
        }

        It "Returns all records when ExcludeId list is empty" {
            $connection = getMockConnection
            
            # Create test records
            @(
                @{ firstname = "User1"; lastname = "Test" }
                @{ firstname = "User2"; lastname = "Test" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Get records with empty ExcludeId
            $results = Get-DataverseRecord -Connection $connection -TableName contact -ExcludeId @() -Columns firstname, lastname
            
            # Assert all records returned
            $results | Should -HaveCount 2
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allContacts | Should -HaveCount 2
        }
    }
}
