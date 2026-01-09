. $PSScriptRoot/Common.ps1

Describe "Remove-DataverseRecord" {    Context "Basic Removal" {
        It "Removes a single record" {
            $connection = getMockConnection
            $record = @{ firstname = "Test"; lastname = "User" }
            $created = $record | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $created.Id | Should -Not -BeNullOrEmpty

            $created | Remove-DataverseRecord -Connection $connection -TableName contact

            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $created.Id -Columns contactid
            $retrieved | Should -BeNullOrEmpty
        }

        It "Removes multiple records in batch" {
            $connection = getMockConnection
            $records = @(
                @{ firstname = "Test1"; lastname = "User1" },
                @{ firstname = "Test2"; lastname = "User2" }
            )
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru -verbose
            $created.Count | Should -Be 2

            $created | Remove-DataverseRecord -Connection $connection -TableName contact -verbose

            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid -verbose
            $remaining.Count | Should -Be 0
        }
    }

    Context "MatchOn Support" {
        It "Removes a single record using MatchOn with single column" {
            $connection = getMockConnection
            
            # Create test records
            $record1 = @{ firstname = "John"; lastname = "Doe"; emailaddress1 = "john@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record2 = @{ firstname = "Jane"; lastname = "Smith"; emailaddress1 = "jane@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Remove using MatchOn
            @{ emailaddress1 = "john@test.com" } | 
                Remove-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1
            
            # Verify only John was deleted
            $remaining = @(Get-DataverseRecord -Connection $connection -TableName contact -Columns emailaddress1)
            $remaining.Count | Should -Be 1
            $remaining[0].emailaddress1 | Should -Be "jane@test.com"
        }

        It "Removes a single record using MatchOn with multiple columns" {
            $connection = getMockConnection
            
            # Create test records with distinct combinations
            $record1 = @{ firstname = "Alice"; lastname = "Brown"; emailaddress1 = "alice.brown@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record2 = @{ firstname = "Bob"; lastname = "Green"; emailaddress1 = "bob.green@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Remove using MatchOn with multiple columns
            @{ firstname = "Alice"; lastname = "Brown" } | 
                Remove-DataverseRecord -Connection $connection -TableName contact -MatchOn @("firstname", "lastname")
            
            # Verify only Alice was deleted
            $remaining = @(Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname)
            $remaining.Count | Should -Be 1
            $remaining[0].firstname | Should -Be "Bob"
        }

        It "Raises error when MatchOn finds multiple matches without AllowMultipleMatches" {
            $connection = getMockConnection
            
            # Create multiple records with same email pattern
            $record1 = @{ firstname = "John1"; lastname = "Doe"; emailaddress1 = "test@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record2 = @{ firstname = "John2"; lastname = "Doe"; emailaddress1 = "test@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Try to remove without AllowMultipleMatches - should error
            {
                @{ emailaddress1 = "test@test.com" } | 
                    Remove-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -ErrorAction Stop
            } | Should -Throw "*AllowMultipleMatches*"
            
            # Verify no records were deleted
            $remaining = @(Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid)
            $remaining.Count | Should -Be 2
        }

        It "Removes multiple records with AllowMultipleMatches switch" {
            $connection = getMockConnection
            
            # Create multiple records with same last name
            $record1 = @{ firstname = "John"; lastname = "TestUser"; emailaddress1 = "john@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record2 = @{ firstname = "Jane"; lastname = "TestUser"; emailaddress1 = "jane@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $record3 = @{ firstname = "Bob"; lastname = "Different"; emailaddress1 = "bob@test.com" } | 
                Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Remove all matching records
            @{ lastname = "TestUser" } | 
                Remove-DataverseRecord -Connection $connection -TableName contact -MatchOn lastname -AllowMultipleMatches
            
            # Verify only Bob remains
            $remaining = @(Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname)
            $remaining.Count | Should -Be 1
            $remaining[0].firstname | Should -Be "Bob"
        }

        It "Does not raise error when MatchOn finds no matches with IfExists" {
            $connection = getMockConnection
            
            # Try to remove non-existent record
            @{ emailaddress1 = "nonexistent@test.com" } | 
                Remove-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -IfExists
            
            # Should complete without error (verified by not throwing)
        }

        It "Raises error when MatchOn finds no matches without IfExists" {
            $connection = getMockConnection
            
            # Try to remove non-existent record without IfExists
            {
                @{ emailaddress1 = "nonexistent@test.com" } | 
                    Remove-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -ErrorAction Stop
            } | Should -Throw "*No records found*"
        }

        It "Raises error when neither Id nor MatchOn is specified" {
            $connection = getMockConnection
            
            # Try to remove without Id or MatchOn
            {
                @{ firstname = "Test" } | 
                    Remove-DataverseRecord -Connection $connection -TableName contact -ErrorAction Stop
            } | Should -Throw "*Either Id or MatchOn must be specified*"
        }
    }

    Context "Retries" {
        It "Retries whole batch on ExecuteMultiple failure" {
            # Set up interceptor for delete operations
            $state = [PSCustomObject]@{ FailCount = 0 }
            $interceptor = {
                param($request)
                write-host "DEBUG: Interceptor called for request of type $($request.GetType().FullName)"

                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    # Only fail delete requests
                    $deleteRequest = $request.Requests | Where-Object { $_ -is [Microsoft.Xrm.Sdk.Messages.DeleteRequest] } | Select-Object -First 1
                    write-host "DEBUG: DeleteRequest - Current FailCount is $($state.FailCount)"
                    if ($deleteRequest -and $state.FailCount -lt 1) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create records first
            $records = @(
                [PSCustomObject]@{ firstname = "John1"; lastname = "Doe1" },
                [PSCustomObject]@{ firstname = "John2"; lastname = "Doe2" }
            )
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru -verbose

            $created[0] | format-list Id, firstname, lastname | Out-Host
            $created[1] | format-list Id, firstname, lastname | Out-Host

            # Now remove with failure
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -Retries 1 -InitialRetryDelay 0.1 -Verbose

            # Check they are deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid -verbose 
            $remaining.Count | Should -Be 0
        }

        It "Retries individual failed items in batch" {
            # Set up interceptor to fail on first ExecuteMultiple request
            $state = [PSCustomObject]@{ FailCount = 0 }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    # Only fail delete requests
                    $deleteRequest = $request.Requests | Where-Object { $_ -is [Microsoft.Xrm.Sdk.Messages.DeleteRequest] } | Select-Object -First 1
                    if ($deleteRequest -and $state.FailCount -lt 1) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create records first
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru -verbose

            # Now remove with failure on first item
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -Retries 1 -InitialRetryDelay 0.1 -Verbose

            # Check they are deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid -verbose
            $remaining.Count | Should -Be 0
        }

        It "Emits errors for all records when batch retries are exceeded" {
            # Set up interceptor to fail more times than retries
            $state = [PSCustomObject]@{ FailCount = 0 }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    # Only fail delete requests
                    $deleteRequest = $request.Requests | Where-Object { $_ -is [Microsoft.Xrm.Sdk.Messages.DeleteRequest] } | Select-Object -First 1
                    if ($deleteRequest -and $state.FailCount -lt 3) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create records first
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru -verbose

            # Now remove with failure exceeding retries
            $errors = @()
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -Retries 1 -InitialRetryDelay 0.1 -verbose -ErrorVariable errors -ErrorAction SilentlyContinue

            $errors.Count | Should -Be 2

            # Verify records are still there
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid -verbose
            $remaining.Count | Should -Be 2
        }
    }

    Context "Parallel Processing" {
        It "Processes deletes in parallel with MaxDegreeOfParallelism > 1" {
            $connection = getMockConnection
            
            # Create test records
            $records = 1..10 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru -verbose
            $created.Count | Should -Be 10

            # Delete with parallel processing
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -MaxDegreeOfParallelism 2 -verbose

            # Verify all deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid -verbose
            $remaining.Count | Should -Be 0
        }

        It "Works with MaxDegreeOfParallelism 1 (sequential)" {
            $connection = getMockConnection
            
            # Create test records
            $records = 1..5 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $created.Count | Should -Be 5

            # Delete with MaxDegreeOfParallelism 1 (should behave like normal batching)
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -MaxDegreeOfParallelism 1

            # Verify all deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $remaining.Count | Should -Be 0
        }

        It "Combines parallel processing with batching" {
            $connection = getMockConnection
            
            # Create test records
            $records = 1..20 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $created.Count | Should -Be 20

            # Delete with parallel processing and small batch size
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -MaxDegreeOfParallelism 3 -BatchSize 5 -verbose

            # Verify all deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $remaining.Count | Should -Be 0
        }

        It "Handles errors in parallel processing" {
            # Set up interceptor to fail some delete operations
            $state = [PSCustomObject]@{ CallCount = 0 }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.DeleteRequest]) {
                    $state.CallCount++
                    # Fail every 3rd delete request
                    if ($state.CallCount % 3 -eq 0) {
                        throw [Exception]::new("Simulated delete failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create test records
            $records = 1..9 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -PassThru

            # Delete with parallel processing - some will fail
            $errors = @()
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -MaxDegreeOfParallelism 2 -BatchSize 1 -ErrorVariable errors -ErrorAction SilentlyContinue -verbose

            # Should have some errors (every 3rd record fails)
            $errors.Count | Should -BeGreaterThan 0
            
            # Should have deleted some records (not all failed)
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $remaining.Count | Should -BeLessThan 9
        }
    }

}

