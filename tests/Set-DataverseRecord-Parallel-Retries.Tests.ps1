Describe "Set-DataverseRecord parallel with retries" {

    . $PSScriptRoot/Common.ps1

    Context "Retries with parallelism enabled" {
        It "Retries failed operations in parallel mode with batching" {
            # Set up interceptor to fail first attempt, succeed on retry
            $state = [PSCustomObject]@{ 
                FailCount = 0
                CreateCount = 0
            }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    $state.CreateCount += $request.Requests.Count
                    # Fail first batch
                    if ($state.FailCount -eq 0) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated transient failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create records with retry enabled and parallel processing
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )
            $errors = @()
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -Retries 2 -InitialRetryDelay 1 -MaxDegreeOfParallelism 2 -BatchSize 10 -ErrorVariable errors -ErrorAction SilentlyContinue

            # Should succeed after retry
            $errors.Count | Should -Be 0
            
            # Verify the interceptor was called twice (first attempt + 1 retry)
            $state.FailCount | Should -Be 1
            $state.CreateCount | Should -BeGreaterOrEqual 2
            
            # Verify records were created
            $created.Count | Should -Be 2
        }

        It "Retries failed operations in parallel mode without batching" {
            # Test with BatchSize=1 to test non-batched parallel execution
            $state = [PSCustomObject]@{ 
                FailCount = 0
                CreateCount = 0
            }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.CreateRequest]) {
                    $state.CreateCount++
                    # Fail first create only
                    if ($state.FailCount -eq 0) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated transient failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create records with retry enabled and parallel processing, no batching
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" },
                @{ firstname = "John2"; lastname = "Doe2" }
            )
            $errors = @()
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -Retries 2 -InitialRetryDelay 1 -MaxDegreeOfParallelism 2 -BatchSize 1 -ErrorVariable errors -ErrorAction SilentlyContinue

            # One should succeed, one might fail (since we only fail the first one)
            # The exact behavior depends on which worker gets the first request
            $state.FailCount | Should -Be 1
            $state.CreateCount | Should -BeGreaterOrEqual 2
        }

        It "Exhausts retries and reports error in parallel mode" {
            # Set up interceptor to always fail
            $state = [PSCustomObject]@{ FailCount = 0 }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    $state.FailCount++
                    throw [Exception]::new("Persistent failure")
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Try to create records with limited retries
            $records = @(
                @{ firstname = "John1"; lastname = "Doe1" }
            )
            $errors = @()
            $created = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -Retries 2 -InitialRetryDelay 1 -MaxDegreeOfParallelism 2 -BatchSize 10 -ErrorVariable errors -ErrorAction SilentlyContinue

            # Should get errors after exhausting retries
            $errors.Count | Should -BeGreaterThan 0
            
            # Verify multiple attempts were made (initial + 2 retries = 3 total)
            $state.FailCount | Should -Be 3
        }
    }
}
