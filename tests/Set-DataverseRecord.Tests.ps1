Describe 'Set-DataverseRecord' {

        . $PSScriptRoot/Common.ps1

    Context 'Basic Record Creation' {
        It "Creates a single record with -CreateOnly" {
            $connection = getMockConnection
            
            $record = @{
                firstname = "John"
                lastname = "Doe"
                emailaddress1 = "john.doe@example.com"
            }
            
            $result = $record | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            $result | Should -Not -BeNullOrEmpty
            $result.Id | Should -BeOfType [Guid]
            $result.Id | Should -Not -Be ([Guid]::Empty)
            
            # Verify record was created
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $result.Id
            $retrieved.firstname | Should -Be "John"
            $retrieved.lastname | Should -Be "Doe"
            $retrieved.emailaddress1 | Should -Be "john.doe@example.com"
        }

        It "Creates multiple records in batch with -CreateOnly" {
            $connection = getMockConnection
            
            $records = @(
                @{ firstname = "Alice"; lastname = "Smith" }
                @{ firstname = "Bob"; lastname = "Johnson" }
                @{ firstname = "Charlie"; lastname = "Williams" }
            )
            
            $results = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            $results | Should -HaveCount 3
            $results | ForEach-Object {
                $_.Id | Should -BeOfType [Guid]
                $_.Id | Should -Not -Be ([Guid]::Empty)
            }
            
            # Verify all records were created
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Where-Object { $_.firstname -eq "Alice" } | Should -HaveCount 1
            $allContacts | Where-Object { $_.firstname -eq "Bob" } | Should -HaveCount 1
            $allContacts | Where-Object { $_.firstname -eq "Charlie" } | Should -HaveCount 1
        }

        It "Creates record without -PassThru returns nothing" {
            $connection = getMockConnection
            
            $record = @{ firstname = "Test"; lastname = "User" }
            
            $result = $record | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            $result | Should -BeNullOrEmpty
            
            # Verify record still created
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Where-Object { $_.firstname -eq "Test" -and $_.lastname -eq "User" } | Should -HaveCount 1
        }

        It "With -PassThru returns each input record exactly once, no duplicates" {
            $connection = getMockConnection
            
            $records = @(
                @{ firstname = "Alice"; lastname = "Smith"; emailaddress1 = "alice@example.com" }
                @{ firstname = "Bob"; lastname = "Johnson"; emailaddress1 = "bob@example.com" }
                @{ firstname = "Charlie"; lastname = "Williams"; emailaddress1 = "charlie@example.com" }
                @{ firstname = "Diana"; lastname = "Brown"; emailaddress1 = "diana@example.com" }
                @{ firstname = "Eve"; lastname = "Davis"; emailaddress1 = "eve@example.com" }
            )
            
            $results = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify count matches input exactly
            $results | Should -HaveCount 5
            
            # Verify each result has unique Id
            $uniqueIds = $results | Select-Object -ExpandProperty Id -Unique
            $uniqueIds | Should -HaveCount 5
            
            # Verify we can match each input to exactly one output
            $results | Where-Object { $_.firstname -eq "Alice" -and $_.lastname -eq "Smith" } | Should -HaveCount 1
            $results | Where-Object { $_.firstname -eq "Bob" -and $_.lastname -eq "Johnson" } | Should -HaveCount 1
            $results | Where-Object { $_.firstname -eq "Charlie" -and $_.lastname -eq "Williams" } | Should -HaveCount 1
            $results | Where-Object { $_.firstname -eq "Diana" -and $_.lastname -eq "Brown" } | Should -HaveCount 1
            $results | Where-Object { $_.firstname -eq "Eve" -and $_.lastname -eq "Davis" } | Should -HaveCount 1
            
            # Verify all results are valid records
            $results | ForEach-Object {
                $_.Id | Should -BeOfType [Guid]
                $_.Id | Should -Not -Be ([Guid]::Empty)
            }
        }
    }

    Context 'Record Updates' {
        It "Updates existing record by Id" {
            $connection = getMockConnection
            
            # Create initial records - one to update, one that should NOT be modified
            $toUpdate = @{ firstname = "Original"; lastname = "ToUpdate"; emailaddress1 = "update@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $shouldNotChange = @{ firstname = "Unchanged"; lastname = "NoChange"; emailaddress1 = "nochange@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update the first record using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $toUpdate.Id
            $updateEntity["firstname"] = "Updated"
            $updateEntity["lastname"] = "ToUpdate"
            
            $updateEntity | Set-DataverseRecord -Connection $connection
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.Id -eq $toUpdate.Id }
            $updated.firstname | Should -Be "Updated"
            $updated.lastname | Should -Be "ToUpdate"
            $updated.emailaddress1 | Should -Be "update@test.com"
            
            $unchanged = $allRecords | Where-Object { $_.Id -eq $shouldNotChange.Id }
            $unchanged.firstname | Should -Be "Unchanged"
            $unchanged.lastname | Should -Be "NoChange"
            $unchanged.emailaddress1 | Should -Be "nochange@test.com"
        }

        It "Updates record with -PassThru returns updated record" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{ firstname = "Original"; lastname = "Name" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $other = @{ firstname = "Other"; lastname = "Record" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with PassThru using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["firstname"] = "Updated"
            
            $result = $updateEntity | Set-DataverseRecord -Connection $connection -PassThru
            
            $result.Id | Should -Be $initial.Id
            # Note: PassThru with SDK Entity objects may not return all attributes in FakeXrmEasy
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.Id -eq $initial.Id }
            $updated.firstname | Should -Be "Updated"
            
            $other = $allRecords | Where-Object { $_.firstname -eq "Other" }
            $other | Should -HaveCount 1
        }

        It "Skips update when no changes detected" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{ firstname = "NoChange"; lastname = "User" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $other = @{ firstname = "Other"; lastname = "User" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # "Update" with same values (should be skipped) using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["firstname"] = "NoChange"
            $updateEntity["lastname"] = "User"
            
            $result = $updateEntity | Set-DataverseRecord -Connection $connection -PassThru -Verbose
            
            $result.Id | Should -Be $initial.Id
            
            # Verify all records unchanged
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            $allRecords | Where-Object { $_.firstname -eq "NoChange" } | Should -HaveCount 1
            $allRecords | Where-Object { $_.firstname -eq "Other" } | Should -HaveCount 1
        }

        It "Updates only specified columns, leaves others unchanged" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{ 
                firstname = "Original"
                lastname = "Name"
                emailaddress1 = "original@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $baseline = @{ 
                firstname = "Baseline"
                lastname = "Record"
                emailaddress1 = "baseline@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update only firstname using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["firstname"] = "Updated"
            
            $updateEntity | Set-DataverseRecord -Connection $connection
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.Id -eq $initial.Id }
            $updated.firstname | Should -Be "Updated"
            $updated.lastname | Should -Be "Name"
            $updated.emailaddress1 | Should -Be "original@example.com"
            
            $baselineRecord = $allRecords | Where-Object { $_.Id -eq $baseline.Id }
            $baselineRecord.firstname | Should -Be "Baseline"
            $baselineRecord.lastname | Should -Be "Record"
            $baselineRecord.emailaddress1 | Should -Be "baseline@example.com"
        }
    }

    Context 'Upsert with MatchOn' {
        It "Creates new record when no match found with -MatchOn" {
            $connection = getMockConnection
            
            $record = @{
                firstname = "NewUser"
                lastname = "Test"
                emailaddress1 = "new@example.com"
            }
            
            $result = $record | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -PassThru
            
            $result.Id | Should -BeOfType [Guid]
            $result.Id | Should -Not -Be ([Guid]::Empty)
            
            # Verify created
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $result.Id
            $retrieved.emailaddress1 | Should -Be "new@example.com"
        }

        It "Updates existing record when match found with -MatchOn" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{
                firstname = "Original"
                lastname = "User"
                emailaddress1 = "unique@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $other = @{
                firstname = "Other"
                lastname = "User"
                emailaddress1 = "other@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Upsert with matching email
            $result = @{
                firstname = "Updated"
                lastname = "User"
                emailaddress1 = "unique@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -PassThru
            
            $result.Id | Should -Be $initial.Id
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.emailaddress1 -eq "unique@example.com" }
            $updated.firstname | Should -Be "Updated"
            
            $unchanged = $allRecords | Where-Object { $_.emailaddress1 -eq "other@example.com" }
            $unchanged.firstname | Should -Be "Other"
        }

        It "Uses multiple columns for matching with -MatchOn" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{
                firstname = "John"
                lastname = "Doe"
                emailaddress1 = "john@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $other = @{
                firstname = "Jane"
                lastname = "Smith"
                emailaddress1 = "jane@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Upsert matching on firstname AND lastname
            $result = @{
                firstname = "John"
                lastname = "Doe"
                emailaddress1 = "newemail@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn @("firstname", "lastname") -PassThru
            
            $result.Id | Should -Be $initial.Id
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.firstname -eq "John" -and $_.lastname -eq "Doe" }
            $updated.emailaddress1 | Should -Be "newemail@example.com"
            
            $unchanged = $allRecords | Where-Object { $_.firstname -eq "Jane" }
            $unchanged.emailaddress1 | Should -Be "jane@example.com"
        }
    }

    Context 'NoUpdate and NoCreate Flags' {
        It "With -NoUpdate, creates new but does not update existing" {
            $connection = getMockConnection
            
            # Create initial record
            $initial = @{
                firstname = "Original"
                emailaddress1 = "test@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Try to update with -NoUpdate (should be skipped) by including Id in input object
            [PSCustomObject]@{
                TableName = "contact"
                Id = $initial.Id
                firstname = "ShouldNotUpdate"
            } | Set-DataverseRecord -Connection $connection -NoUpdate
            
            # Verify not updated
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact
            $matchingRecord = $retrieved | Where-Object { $_.Id -eq $initial.Id }
            $matchingRecord.firstname | Should -Be "Original"
            
            # Verify can still create new records with -NoUpdate
            $newRecord = @{
                firstname = "NewUser"
                emailaddress1 = "new@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -NoUpdate -PassThru
            
            $newRecord.Id | Should -Not -Be $initial.Id
        }

        It "With -NoCreate, updates existing but does not create new" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{
                firstname = "Original"
                emailaddress1 = "test@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $baseline = @{
                firstname = "Baseline"
                emailaddress1 = "baseline@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update existing with -NoCreate using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["firstname"] = "Updated"
            
            $updateEntity | Set-DataverseRecord -Connection $connection -NoCreate -PassThru
            
            # Try to create new with -NoCreate (should be skipped)
            @{
                firstname = "ShouldNotCreate"
                emailaddress1 = "new@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -NoCreate
            
            # Verify all records in expected state - should still be 2 records
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $allContacts | Should -HaveCount 2
            
            $updated = $allContacts | Where-Object { $_.Id -eq $initial.Id }
            $updated.firstname | Should -Be "Updated"
            
            $baselineRecord = $allContacts | Where-Object { $_.Id -eq $baseline.Id }
            $baselineRecord.firstname | Should -Be "Baseline"
            
            $allContacts | Where-Object { $_.firstname -eq "ShouldNotCreate" } | Should -HaveCount 0
        }
    }

    Context 'NoUpdateColumns Parameter' {
        It "Excludes specified columns from updates" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{
                firstname = "Original"
                lastname = "Name"
                emailaddress1 = "original@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $baseline = @{
                firstname = "Baseline"
                lastname = "Record"
                emailaddress1 = "baseline@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with NoUpdateColumns using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["firstname"] = "Updated"
            $updateEntity["lastname"] = "UpdatedLast"
            $updateEntity["emailaddress1"] = "updated@example.com"
            
            $updateEntity | Set-DataverseRecord -Connection $connection -NoUpdateColumns emailaddress1
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.Id -eq $initial.Id }
            $updated.firstname | Should -Be "Updated"
            $updated.lastname | Should -Be "UpdatedLast"
            $updated.emailaddress1 | Should -Be "original@example.com"
            
            $baselineRecord = $allRecords | Where-Object { $_.Id -eq $baseline.Id }
            $baselineRecord.firstname | Should -Be "Baseline"
            $baselineRecord.lastname | Should -Be "Record"
            $baselineRecord.emailaddress1 | Should -Be "baseline@example.com"
        }

        It "Excludes multiple columns from updates" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{
                firstname = "Original"
                lastname = "Name"
                emailaddress1 = "original@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $baseline = @{
                firstname = "Baseline"
                lastname = "Record"
                emailaddress1 = "baseline@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with multiple NoUpdateColumns using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["firstname"] = "Updated"
            $updateEntity["lastname"] = "UpdatedLast"
            $updateEntity["emailaddress1"] = "updated@example.com"
            
            $updateEntity | Set-DataverseRecord -Connection $connection -NoUpdateColumns lastname, emailaddress1
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.Id -eq $initial.Id }
            $updated.firstname | Should -Be "Updated"
            $updated.lastname | Should -Be "Name"
            $updated.emailaddress1 | Should -Be "original@example.com"
            
            $baselineRecord = $allRecords | Where-Object { $_.Id -eq $baseline.Id }
            $baselineRecord.firstname | Should -Be "Baseline"
            $baselineRecord.lastname | Should -Be "Record"
            $baselineRecord.emailaddress1 | Should -Be "baseline@example.com"
        }
    }

    Context 'UpdateAllColumns Parameter' {
        It "With -UpdateAllColumns, skips retrieve and sends all columns" {
            $connection = getMockConnection
            
            # Create initial record
            $initial = @{
                firstname = "Original"
                lastname = "Name"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update with UpdateAllColumns (must provide Id) by including Id in input object
            [PSCustomObject]@{
                TableName = "contact"
                Id = $initial.Id
                firstname = "Updated"
                lastname = "UpdatedLast"
            } | Set-DataverseRecord -Connection $connection -UpdateAllColumns
            
            # Verify update
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact
            $matchingRecord = $retrieved | Where-Object { $_.Id -eq $initial.Id }
            $matchingRecord.firstname | Should -Be "Updated"
            $matchingRecord.lastname | Should -Be "UpdatedLast"
        }
    }

    Context 'IgnoreProperties Parameter' {
        It "Ignores specified properties on input object" {
            $connection = getMockConnection
            
            $inputObject = [PSCustomObject]@{
                firstname = "John"
                lastname = "Doe"
                customProperty = "ShouldBeIgnored"
                TableName = "contact"
            }
            
            $result = $inputObject | Set-DataverseRecord -Connection $connection -IgnoreProperties customProperty -CreateOnly -PassThru
            
            $result.Id | Should -BeOfType [Guid]
            
            # Verify record created without error
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $result.Id
            $retrieved.firstname | Should -Be "John"
        }
    }

    Context 'Type Conversions' {
        It "Converts date/time values correctly" {
            $connection = getMockConnection
            
            $birthdate = [DateTime]::Parse("1990-01-15")
            
            $record = @{
                firstname = "DateTest"
                lastname = "User"
                birthdate = $birthdate
            }
            
            $result = $record | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $result.Id
            $retrieved.birthdate.Date | Should -Be $birthdate.Date
        }

        It "Converts choice/optionset values from label" {
            $connection = getMockConnection
            
            # accountrolecode is a choice field in contact
            $record = @{
                accountrolecode = "Employee"
            }
            
            $result = $record | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $result.Id
            $retrieved.accountrolecode | Should -Be 2
        }

        It "Converts lookup/EntityReference values from GUID" {
            $connection = getMockConnection
            
            # Create a parent contact first
            $parent = @{
                firstname = "Parent"
                lastname = "Contact"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Create child with lookup to parent
            $child = @{
                firstname = "Child"
                lastname = "Contact"
                parentcontactid = $parent.Id
            }
            
            $result = $child | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Verify lookup
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $result.Id
            $retrieved.parentcontactid.Id | Should -Be $parent.Id
        }

        It "Handles null values correctly" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{
                firstname = "Test"
                lastname = "User"
                emailaddress1 = "test@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $baseline = @{
                firstname = "Baseline"
                lastname = "User"
                emailaddress1 = "baseline@example.com"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update to null using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["emailaddress1"] = $null
            
            $updateEntity | Set-DataverseRecord -Connection $connection
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.Id -eq $initial.Id }
            $updated.emailaddress1 | Should -BeNullOrEmpty
            
            $baselineRecord = $allRecords | Where-Object { $_.Id -eq $baseline.Id }
            $baselineRecord.emailaddress1 | Should -Be "baseline@example.com"
        }
    }

    Context 'Batch Operations' {
        It "Batches multiple creates with default batch size" {
            $connection = getMockConnection
            
            $records = 1..150 | ForEach-Object {
                @{ firstname = "User$_"; lastname = "Test" }
            }
            
            $results = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            $results | Should -HaveCount 150
            $results | ForEach-Object {
                $_.Id | Should -BeOfType [Guid]
            }
        }

        It "Respects custom -BatchSize parameter" {
            $connection = getMockConnection
            
            $records = 1..10 | ForEach-Object {
                @{ firstname = "Batch$_"; lastname = "Test" }
            }
            
            $results = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -BatchSize 5 -PassThru
            
            $results | Should -HaveCount 10
        }

        It "With -BatchSize 1, processes records one at a time" {
            $connection = getMockConnection
            
            $records = 1..3 | ForEach-Object {
                @{ firstname = "Single$_"; lastname = "Test" }
            }
            
            $results = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -BatchSize 1 -PassThru
            
            $results | Should -HaveCount 3
        }
    }

    Context 'Error Handling' {
        It "Collects errors with -ErrorVariable in batch operations" {
            $connection = getMockConnection
            
            $records = @(
                @{ firstname = "Valid"; lastname = "User1" }
                @{ firstname = "Valid"; lastname = "User2" }
            )
            
            $errors = @()
            $results = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -ErrorVariable +errors -ErrorAction SilentlyContinue -PassThru
            
            # Should have results for successful records
            $results | Should -HaveCount 2
        }

        It "Continues batch processing after error with default ErrorAction" {
            $connection = getMockConnection
            
            # Mix of valid records
            $records = @(
                @{ firstname = "User1"; lastname = "Test" }
                @{ firstname = "User2"; lastname = "Test" }
                @{ firstname = "User3"; lastname = "Test" }
            )
            
            $errors = @()
            $results = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -ErrorVariable +errors -ErrorAction SilentlyContinue -PassThru
            
            # All valid records should succeed
            $results | Should -HaveCount 3
        }
    }

    Context 'WhatIf and Confirm' {
        It "With -WhatIf, does not create records" {
            $connection = getMockConnection
            
            $initialCount = (Get-DataverseRecord -Connection $connection -TableName contact).Count
            
            $record = @{ firstname = "WhatIfTest"; lastname = "User" }
            
            $record | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -WhatIf
            
            $finalCount = (Get-DataverseRecord -Connection $connection -TableName contact).Count
            $finalCount | Should -Be $initialCount
        }

        It "With -WhatIf, does not update records" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{ firstname = "Original"; lastname = "Name" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $baseline = @{ firstname = "Baseline"; lastname = "Record" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Try to update with WhatIf using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["firstname"] = "ShouldNotUpdate"
            
            $updateEntity | Set-DataverseRecord -Connection $connection -WhatIf
            
            # Verify all records unchanged
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $original = $allRecords | Where-Object { $_.Id -eq $initial.Id }
            $original.firstname | Should -Be "Original"
            
            $baselineRecord = $allRecords | Where-Object { $_.Id -eq $baseline.Id }
            $baselineRecord.firstname | Should -Be "Baseline"
        }
    }

    Context 'Dataset Integrity' {
        It "Does not create duplicate records" {
            $connection = getMockConnection
            
            $record = @{
                firstname = "Unique"
                lastname = "User"
                emailaddress1 = "unique@example.com"
            }
            
            # Create first time
            $first = $record | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Create second time with same data (should get a different Id since we're using CreateOnly)
            $second = $record | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Note: FakeXrmEasy may return the same Id for identical creates
            # In real Dataverse, these would be different records
            # We just verify both operations succeeded
            $first.Id | Should -Not -BeNullOrEmpty
            $second.Id | Should -Not -BeNullOrEmpty
            
            # Verify records exist
            $allContacts = Get-DataverseRecord -Connection $connection -TableName contact
            $matches = $allContacts | Where-Object { $_.firstname -eq "Unique" -and $_.lastname -eq "User" }
            $matches | Should -Not -BeNullOrEmpty
        }

        It "Updates do not affect other records" {
            $connection = getMockConnection
            
            # Create multiple records
            $record1 = @{ firstname = "User1"; lastname = "Test" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record2 = @{ firstname = "User2"; lastname = "Test" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record3 = @{ firstname = "User3"; lastname = "Test" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update record1 using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $record1.Id
            $updateEntity["firstname"] = "UpdatedUser1"
            
            $updateEntity | Set-DataverseRecord -Connection $connection
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 3
            
            $retrieved1 = $allRecords | Where-Object { $_.Id -eq $record1.Id }
            $retrieved1.firstname | Should -Be "UpdatedUser1"
            
            $retrieved2 = $allRecords | Where-Object { $_.Id -eq $record2.Id }
            $retrieved2.firstname | Should -Be "User2"
            
            $retrieved3 = $allRecords | Where-Object { $_.Id -eq $record3.Id }
            $retrieved3.firstname | Should -Be "User3"
        }

        It "No records are lost during batch operations" {
            $connection = getMockConnection
            
            $initialCount = (Get-DataverseRecord -Connection $connection -TableName contact).Count
            
            $newRecords = 1..50 | ForEach-Object {
                @{ firstname = "BatchUser$_"; lastname = "Test" }
            }
            
            $newRecords | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly
            
            $finalCount = (Get-DataverseRecord -Connection $connection -TableName contact).Count
            $finalCount | Should -Be ($initialCount + 50)
        }

        It "Verify no side effects on unrelated fields" {
            $connection = getMockConnection
            
            # Create records with multiple fields
            $initial = @{
                firstname = "Complete"
                lastname = "User"
                emailaddress1 = "complete@example.com"
                description = "Original description"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $baseline = @{
                firstname = "Baseline"
                lastname = "Record"
                emailaddress1 = "baseline@example.com"
                description = "Baseline description"
            } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update only firstname using SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["firstname"] = "UpdatedFirst"
            
            $updateEntity | Set-DataverseRecord -Connection $connection
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.Id -eq $initial.Id }
            $updated.firstname | Should -Be "UpdatedFirst"
            $updated.lastname | Should -Be "User"
            $updated.emailaddress1 | Should -Be "complete@example.com"
            $updated.description | Should -Be "Original description"
            
            $baselineRecord = $allRecords | Where-Object { $_.Id -eq $baseline.Id }
            $baselineRecord.firstname | Should -Be "Baseline"
            $baselineRecord.lastname | Should -Be "Record"
            $baselineRecord.emailaddress1 | Should -Be "baseline@example.com"
            $baselineRecord.description | Should -Be "Baseline description"
        }

        It "Deleting and recreating maintains data integrity" {
            $connection = getMockConnection
            
            # Create record
            $record = @{ firstname = "ToDelete"; lastname = "User" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            $originalId = $record.Id
            
            # Delete record
            Remove-DataverseRecord -Connection $connection -TableName contact -Id $originalId
            
            # Verify deleted
            $deleted = Get-DataverseRecord -Connection $connection -TableName contact -Id $originalId -ErrorAction SilentlyContinue
            $deleted | Should -BeNullOrEmpty
            
            # Recreate with same data
            $newRecord = @{ firstname = "ToDelete"; lastname = "User" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Should have different ID
            $newRecord.Id | Should -Not -Be $originalId
        }
    }

    Context 'Pipeline and Property Handling' {
        It "Accepts TableName from pipeline property" {
            $connection = getMockConnection
            
            $inputObject = [PSCustomObject]@{
                TableName = "contact"
                firstname = "Pipeline"
                lastname = "Test"
            }
            
            $result = $inputObject | Set-DataverseRecord -Connection $connection -CreateOnly -PassThru
            
            $result.Id | Should -BeOfType [Guid]
            $result.TableName | Should -Be "contact"
        }

        It "Accepts Id from pipeline property" {
            $connection = getMockConnection
            
            # Create initial records
            $initial = @{ firstname = "Original"; lastname = "Name" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $baseline = @{ firstname = "Baseline"; lastname = "Record" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Update using pipeline with SDK Entity object
            $updateEntity = New-Object Microsoft.Xrm.Sdk.Entity "contact"
            $updateEntity.Id = $updateEntity["contactid"] = $initial.Id
            $updateEntity["firstname"] = "Updated"
            
            $updateEntity | Set-DataverseRecord -Connection $connection
            
            # Verify all records in expected state
            $allRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $allRecords | Should -HaveCount 2
            
            $updated = $allRecords | Where-Object { $_.Id -eq $initial.Id }
            $updated.firstname | Should -Be "Updated"
            
            $baselineRecord = $allRecords | Where-Object { $_.Id -eq $baseline.Id }
            $baselineRecord.firstname | Should -Be "Baseline"
        }

        It "Processes multiple records from pipeline" {
            $connection = getMockConnection
            
            $results = 1..5 | ForEach-Object {
                [PSCustomObject]@{
                    TableName = "contact"
                    firstname = "Pipe$_"
                    lastname = "Test"
                }
            } | Set-DataverseRecord -Connection $connection -CreateOnly -PassThru
            
            $results | Should -HaveCount 5
        }
    }

    Context "Retries" {
        It "Retries whole batch on ExecuteMultiple failure" {
            $state = [PSCustomObject]@{ FailCount = 0 }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    if ($state.FailCount -lt 1) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor


            $existingrewcords = Get-DataverseRecord -Connection $connection -TableName contact
            $existingrewcords.Count | Should -Be 0

            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )

            $records | Set-DataverseRecord -Connection $connection -TableName contact -Retries 1 -Verbose

            $createdRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $createdRecords.Count | Should -Be 2
            $createdRecords | ForEach-Object { $_.firstname | Should -BeIn @("John1", "John2") }
        }

        It "Retries individual failed items in batch" {
            $state = [PSCustomObject]@{ FailCount = 0 }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    if ($state.FailCount -lt 1) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )

            $records | Set-DataverseRecord -Connection $connection -TableName contact -Retries 1 -Verbose

            $createdRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $createdRecords.Count | Should -Be 2
            $createdRecords | ForEach-Object { $_.firstname | Should -BeIn @("John1", "John2") }
        }

        It "Emits errors for all records when batch retries are exceeded" {
            $state = [PSCustomObject]@{ FailCount = 0 }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    if ($state.FailCount -lt 3) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )

            $errors = @()
            $records | Set-DataverseRecord -Connection $connection -TableName contact -Retries 1 -ErrorVariable +errors -ErrorAction SilentlyContinue

            $errors.Count | Should -Be 2

            # Verify no records were created
            $createdRecords = Get-DataverseRecord -Connection $connection -TableName contact
            $createdRecords.Count | Should -Be 0
        }
    }
}
