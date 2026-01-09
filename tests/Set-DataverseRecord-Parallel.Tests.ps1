. $PSScriptRoot/Common.ps1

Describe "Set-DataverseRecord Parallel Processing" {    Context "Parallel Processing" {
        It "Processes creates in parallel with MaxDegreeOfParallelism > 1" {
            $connection = getMockConnection
            
            # Create test records in parallel
            $records = 1..10 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -MaxDegreeOfParallelism 2 -verbose
            $created.Count | Should -Be 10

            # Verify all created
            $retrieved = @(Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid -verbose)
            $retrieved.Count | Should -Be 10
        }

        It "Works with MaxDegreeOfParallelism 1 (sequential)" {
            $connection = getMockConnection
            
            # Create test records
            $records = 1..5 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -MaxDegreeOfParallelism 1
            $created.Count | Should -Be 5

            # Verify all created
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $retrieved.Count | Should -Be 5
        }

        It "Combines parallel processing with batching" {
            $connection = getMockConnection
            
            # Create test records
            $records = 1..20 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -MaxDegreeOfParallelism 3 -BatchSize 5 -verbose
            $created.Count | Should -Be 20

            # Verify all created
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $retrieved.Count | Should -Be 20
        }

        It "Processes updates in parallel" {
            $connection = getMockConnection
            
            # Create test records first
            $records = 1..10 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $created.Count | Should -Be 10

            # Update them in parallel
            $updates = $created | ForEach-Object {
                @{ Id = $_.Id; firstname = "Updated$($_.firstname)"; lastname = $_.lastname }
            }
            $updated = $updates | Set-DataverseRecord -Connection $connection -TableName contact -PassThru -MaxDegreeOfParallelism 2 -verbose
            $updated.Count | Should -Be 10

            # Verify updates
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname
            $retrieved | Where-Object { $_.firstname -like "UpdatedTest*" } | Should -HaveCount 10
        }

        It "Handles errors in parallel processing" {
            # Set up interceptor to fail some create operations
            $state = [PSCustomObject]@{ CallCount = 0 }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.CreateRequest]) {
                    $state.CallCount++
                    # Fail every 3rd create request
                    if ($state.CallCount % 3 -eq 0) {
                        throw [Exception]::new("Simulated create failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create test records
            $records = 1..9 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }

            # Set with parallel processing - some will fail
            $errors = @()
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -MaxDegreeOfParallelism 2 -BatchSize 1 -ErrorVariable errors -ErrorAction SilentlyContinue -verbose

            # Should have some errors (every 3rd record fails)
            $errors.Count | Should -BeGreaterThan 0
            
            # Should have created some records (not all failed)
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $retrieved.Count | Should -BeLessThan 9
            $retrieved.Count | Should -BeGreaterThan 0
        }

        It "Processes upserts in parallel" {
            $connection = getMockConnection
            
            # Create test records with upsert
            $records = 1..10 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -Upsert -PassThru -MaxDegreeOfParallelism 2 -verbose
            $created.Count | Should -Be 10

            # Verify all created
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $retrieved.Count | Should -Be 10

            # Upsert again with updates (should update existing records)
            $updates = $created | ForEach-Object {
                @{ Id = $_.Id; firstname = "Updated$($_.firstname)"; lastname = $_.lastname }
            }
            $upserted = $updates | Set-DataverseRecord -Connection $connection -TableName contact -Upsert -PassThru -MaxDegreeOfParallelism 2 -verbose
            $upserted.Count | Should -Be 10

            # Verify still 10 records (no duplicates)
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid
            $retrieved.Count | Should -Be 10
        }

        It "Respects PassThru parameter in parallel mode" {
            $connection = getMockConnection
            
            # Create test records
            $records = 1..5 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User$_" }
            }
            
            # Without PassThru
            $withoutPassThru = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -MaxDegreeOfParallelism 2
            $withoutPassThru | Should -BeNullOrEmpty

            # Clear records
            Get-DataverseRecord -Connection $connection -TableName contact -Columns contactid | Remove-DataverseRecord -Connection $connection -TableName contact

            # With PassThru
            $withPassThru = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -MaxDegreeOfParallelism 2
            $withPassThru.Count | Should -Be 5
            $withPassThru | ForEach-Object { $_.Id | Should -Not -BeNullOrEmpty }
        }

        It "Works with MatchOn in parallel mode" {
            $connection = getMockConnection
            
            # Create initial records
            $records = 1..5 | ForEach-Object {
                @{ firstname = "Test$_"; lastname = "User"; emailaddress1 = "test$_@example.com" }
            }
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru

            # Update using MatchOn in parallel
            $updates = 1..5 | ForEach-Object {
                @{ emailaddress1 = "test$_@example.com"; firstname = "Updated$_"; lastname = "User" }
            }
            $updated = $updates | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn emailaddress1 -PassThru -MaxDegreeOfParallelism 2 -verbose
            $updated.Count | Should -Be 5

            # Verify updates
            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Columns firstname
            $retrieved.Count | Should -Be 5
            $retrieved | Where-Object { $_.firstname -like "Updated*" } | Should -HaveCount 5
        }
    }

    Context "Parallel Processing Limitations" {
        It "Retries in parallel mode with batching - transient failures are retried" {
            # NOTE: Retries are now supported in parallel mode when batching is enabled (BatchSize > 1).
            # Each worker has its own batch processor that handles retries independently.
            
            # Set up interceptor to fail first batch attempt
            $state = [PSCustomObject]@{ FailCount = 0; CreateCount = 0 }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    $state.CreateCount += $request.Requests.Count
                    if ($state.FailCount -lt 1) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated transient failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Try to create records with retry enabled and BatchSize > 1 (batching enabled)
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )
            $errors = @()
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -Retries 5 -InitialRetryDelay 0.1 -MaxDegreeOfParallelism 2 -BatchSize 10 -ErrorVariable errors -ErrorAction SilentlyContinue

            # Should succeed after retry - no errors expected
            $errors.Count | Should -Be 0
            
            # Verify the interceptor was called multiple times (initial + retries)
            # We expect at least 2 calls: one failure + one success on retry
            $state.CreateCount | Should -BeGreaterOrEqual 2
            
            # Verify both records were created
            $created.Count | Should -Be 2
        }
    }

}

