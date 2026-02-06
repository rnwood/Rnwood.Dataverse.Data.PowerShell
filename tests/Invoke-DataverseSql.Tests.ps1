. $PSScriptRoot/Common.ps1

Describe 'Invoke-DataverseSql' {
    Context 'Parameter Binding' {
        It "Accepts SQL as positional parameter" {
            # Test that the SQL parameter can be provided positionally without -Sql flag
            # This test validates parameter binding, not execution
            $connection = getMockConnection
            
            # Get the cmdlet parameter metadata
            $cmdlet = Get-Command Invoke-DataverseSql
            $sqlParam = $cmdlet.Parameters['Sql']
            
            # Verify Sql parameter has Position = 0
            $sqlParam.Attributes | Where-Object { $_ -is [System.Management.Automation.ParameterAttribute] } | ForEach-Object {
                $_.Position | Should -Be 0
            }
        }
    }
}

Describe 'Invoke-DataverseSql - Full Execution' -Skip {
    # Note: SQL4Cds (MarkMpn.Sql4Cds.Engine) does not fully support FakeXrmEasy mock
    # These tests validate the command signature and expected behavior, but require
    # real Dataverse environment or enhanced mock support to pass.
    # Run these as E2E tests against real environment for full validation.
    
    Context 'SELECT Operations' {
        It "Executes SELECT query and returns PSObjects with correct properties" {
            $connection = getMockConnection
            
            # Create test data
            @(
                @{ firstname = "John"; lastname = "Doe"; emailaddress1 = "john@example.com" }
                @{ firstname = "Jane"; lastname = "Smith"; emailaddress1 = "jane@example.com" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute SELECT query
            $results = Invoke-DataverseSql -Connection $connection -Sql "SELECT firstname, lastname, emailaddress1 FROM contact WHERE lastname = 'Doe'"
            
            # Assert results
            $results | Should -Not -BeNullOrEmpty
            $results | Should -HaveCount 1
            $results[0] | Should -BeOfType [PSCustomObject]
            $results[0].firstname | Should -Be "John"
            $results[0].lastname | Should -Be "Doe"
            $results[0].emailaddress1 | Should -Be "john@example.com"
            
            # Verify no side effects - both records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 2
        }

        It "Executes SELECT with TOP and returns limited results" {
            $connection = getMockConnection
            
            # Create test data
            1..5 | ForEach-Object {
                @{ firstname = "User$_"; lastname = "Test" } | 
                    Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            }
            
            # Execute SELECT with TOP
            $results = Invoke-DataverseSql -Connection $connection -Sql "SELECT TOP 2 firstname, lastname FROM contact"
            
            # Assert results
            $results | Should -Not -BeNullOrEmpty
            $results | Should -HaveCount 2
            
            # Verify no side effects - all 5 records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 5
        }

        It "Executes parameterized SELECT query" {
            $connection = getMockConnection
            
            # Create test data
            @(
                @{ firstname = "Alice"; lastname = "Johnson" }
                @{ firstname = "Bob"; lastname = "Johnson" }
                @{ firstname = "Charlie"; lastname = "Brown" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute parameterized query
            $params = @{ searchLastname = "Johnson" }
            $results = Invoke-DataverseSql -Connection $connection -Sql "SELECT firstname, lastname FROM contact WHERE lastname = @searchLastname" -Parameters $params
            
            # Assert results
            $results | Should -Not -BeNullOrEmpty
            $results | Should -HaveCount 2
            $results | ForEach-Object { $_.lastname | Should -Be "Johnson" }
            ($results | Where-Object { $_.firstname -eq "Alice" }) | Should -HaveCount 1
            ($results | Where-Object { $_.firstname -eq "Bob" }) | Should -HaveCount 1
            
            # Verify no side effects - all 3 records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 3
        }
    }

    Context 'INSERT Operations' {
        It "Executes INSERT statement and creates record" {
            $connection = getMockConnection
            
            # Execute INSERT
            Invoke-DataverseSql -Connection $connection -Sql "INSERT INTO contact (firstname, lastname, emailaddress1) VALUES ('Test', 'User', 'test@example.com')"
            
            # Verify record was created
            $results = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ lastname = "User" } -Columns emailaddress1, firstname, lastname
            $results | Should -Not -BeNullOrEmpty
            $results | Should -HaveCount 1
            $results[0].firstname | Should -Be "Test"
            $results[0].lastname | Should -Be "User"
            $results[0].emailaddress1 | Should -Be "test@example.com"
            
            # Verify only one record was created (no side effects)
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }

        It "Executes parameterized INSERT statement" {
            $connection = getMockConnection
            
            # Execute parameterized INSERT
            $params = @{ 
                fname = "Parameterized"
                lname = "Insert"
                email = "param@example.com"
            }
            Invoke-DataverseSql -Connection $connection -Sql "INSERT INTO contact (firstname, lastname, emailaddress1) VALUES (@fname, @lname, @email)" -Parameters $params
            
            # Verify record was created with correct values
            $results = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ lastname = "Insert" } -Columns emailaddress1, firstname
            $results | Should -Not -BeNullOrEmpty
            $results | Should -HaveCount 1
            $results[0].firstname | Should -Be "Parameterized"
            $results[0].emailaddress1 | Should -Be "param@example.com"
            
            # Verify no side effects
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'UPDATE Operations' {
        It "Executes UPDATE statement and modifies records" {
            $connection = getMockConnection
            
            # Create initial records
            @(
                @{ firstname = "Original1"; lastname = "UpdateTest"; emailaddress1 = "orig1@example.com" }
                @{ firstname = "Original2"; lastname = "UpdateTest"; emailaddress1 = "orig2@example.com" }
                @{ firstname = "NoChange"; lastname = "Different"; emailaddress1 = "nochange@example.com" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute UPDATE
            Invoke-DataverseSql -Connection $connection -Sql "UPDATE contact SET firstname = 'Updated' WHERE lastname = 'UpdateTest'"
            
            # Verify records were updated
            $updatedRecords = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ lastname = "UpdateTest" } -Columns firstname, lastname
            $updatedRecords | Should -HaveCount 2
            $updatedRecords | ForEach-Object { $_.firstname | Should -Be "Updated" }
            
            # Verify unrelated record was not affected (no side effects)
            $unchangedRecord = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ lastname = "Different" } -Columns lastname
            $unchangedRecord | Should -HaveCount 1
            $unchangedRecord[0].firstname | Should -Be "NoChange"
            
            # Verify total count unchanged
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 3
        }

        It "Executes parameterized UPDATE with WHERE clause" {
            $connection = getMockConnection
            
            # Create test record
            $record = @{ firstname = "OldName"; lastname = "UpdateMe"; emailaddress1 = "old@example.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Execute parameterized UPDATE
            $params = @{
                newFirstName = "NewName"
                newEmail = "new@example.com"
                targetId = $record.Id
            }
            Invoke-DataverseSql -Connection $connection -Sql "UPDATE contact SET firstname = @newFirstName, emailaddress1 = @newEmail WHERE contactid = @targetId" -Parameters $params
            
            # Verify record was updated correctly
            $updatedRecord = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id -Columns lastname
            $updatedRecord.firstname | Should -Be "NewName"
            $updatedRecord.emailaddress1 | Should -Be "new@example.com"
            $updatedRecord.lastname | Should -Be "UpdateMe"  # Unchanged field remains
            
            # Verify no side effects - only one record exists
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 1
        }
    }

    Context 'DELETE Operations' {
        It "Executes DELETE statement and removes records" {
            $connection = getMockConnection
            
            # Create test records
            @(
                @{ firstname = "Keep1"; lastname = "KeepMe" }
                @{ firstname = "Delete1"; lastname = "DeleteMe" }
                @{ firstname = "Delete2"; lastname = "DeleteMe" }
                @{ firstname = "Keep2"; lastname = "KeepMe" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute DELETE
            Invoke-DataverseSql -Connection $connection -Sql "DELETE FROM contact WHERE lastname = 'DeleteMe'"
            
            # Verify records were deleted
            $deletedRecords = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ lastname = "DeleteMe" } -Columns contactid
            $deletedRecords | Should -BeNullOrEmpty
            
            # Verify kept records still exist (no side effects)
            $keptRecords = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ lastname = "KeepMe" } -Columns contactid
            $keptRecords | Should -HaveCount 2
            
            # Verify total count
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 2
        }

        It "Executes DELETE with -WhatIf and does not delete records" {
            $connection = getMockConnection
            
            # Create test records
            @(
                @{ firstname = "Test1"; lastname = "WhatIfTest" }
                @{ firstname = "Test2"; lastname = "WhatIfTest" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute DELETE with -WhatIf
            Invoke-DataverseSql -Connection $connection -Sql "DELETE FROM contact WHERE lastname = 'WhatIfTest'" -WhatIf
            
            # Verify NO records were deleted (WhatIf prevents deletion)
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname, lastname
            $allRecords | Should -HaveCount 2
            $allRecords | ForEach-Object { $_.lastname | Should -Be "WhatIfTest" }
        }
    }

    Context 'Pipeline Parameterization' {
        It "Executes parameterized query once per pipeline object" {
            $connection = getMockConnection
            
            # Create test records
            @(
                @{ firstname = "User1"; lastname = "Smith" }
                @{ firstname = "User2"; lastname = "Johnson" }
                @{ firstname = "User3"; lastname = "Smith" }
            ) | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            # Execute pipeline parameterization
            $paramObjects = @(
                @{ searchName = "Smith" }
                @{ searchName = "Johnson" }
            )
            $results = $paramObjects | Invoke-DataverseSql -Connection $connection -Sql "SELECT firstname, lastname FROM contact WHERE lastname = @searchName"
            
            # Assert results - should have results for both queries
            $results | Should -Not -BeNullOrEmpty
            # Smith query returns 2 records, Johnson returns 1, total 3
            $results | Should -HaveCount 3
            
            # Verify both lastnames are present in results
            ($results | Where-Object { $_.lastname -eq "Smith" }) | Should -HaveCount 2
            ($results | Where-Object { $_.lastname -eq "Johnson" }) | Should -HaveCount 1
            
            # Verify no side effects - all records still exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $allContacts | Should -HaveCount 3
        }
    }
}
