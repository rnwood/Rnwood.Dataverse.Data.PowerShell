Describe "Remove-DataverseRecord parallel with retries" {

    . $PSScriptRoot/Common.ps1

    Context "Retries with parallelism enabled" {
        It "Retries failed delete operations in parallel mode with batching" {
            # Set up interceptor to fail first attempt, succeed on retry
            $state = [PSCustomObject]@{ 
                FailCount = 0
                DeleteCount = 0
            }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    $state.DeleteCount += $request.Requests.Count
                    # Fail first batch
                    if ($state.FailCount -eq 0) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated transient failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create records first
            $c1 = @{ firstname = "John1"; lastname = "Doe1" } | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $c2 = @{ firstname = "John2"; lastname = "Doe2" } | Set-DataverseRecord -Connection $connection -TableName contact -PassThru

            # Reset state after creation
            $state.FailCount = 0
            $state.DeleteCount = 0
            
            # Delete records with retry enabled and parallel processing
            $errors = @()
            $c1, $c2 | Remove-DataverseRecord -Connection $connection -TableName contact -Retries 2 -InitialRetryDelay 1 -MaxDegreeOfParallelism 2 -BatchSize 10 -ErrorVariable errors -ErrorAction SilentlyContinue

            # Should succeed after retry
            $errors.Count | Should -Be 0
            
            # Verify the interceptor was called twice (first attempt + 1 retry)
            $state.FailCount | Should -Be 1
            $state.DeleteCount | Should -BeGreaterOrEqual 2
        }

        It "Retries failed delete operations in parallel mode without batching" {
            # Test with BatchSize=1 to test non-batched parallel execution with retries
            $state = [PSCustomObject]@{ 
                FailCount = 0
                DeleteCount = 0
            }
            $interceptor = {
                param($request)
                if ($request -is [Microsoft.Xrm.Sdk.Messages.DeleteRequest]) {
                    $state.DeleteCount++
                    # Fail first 2 attempts, succeed on 3rd
                    if ($state.FailCount -lt 2) {
                        $state.FailCount++
                        throw [Exception]::new("Simulated transient failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create a record first
            $c1 = @{ firstname = "John1"; lastname = "Doe1" } | Set-DataverseRecord -Connection $connection -TableName contact -PassThru

            # Reset state after creation
            $state.FailCount = 0
            $state.DeleteCount = 0
            
            # Delete record with retry enabled and parallel processing, no batching
            $errors = @()
            $c1 | Remove-DataverseRecord -Connection $connection -TableName contact -Retries 3 -InitialRetryDelay 1 -MaxDegreeOfParallelism 2 -BatchSize 1 -ErrorVariable errors -ErrorAction SilentlyContinue

            # Should succeed after retries
            $errors.Count | Should -Be 0
            
            # Verify retries happened (2 failures + 1 success = 3 attempts)
            $state.FailCount | Should -Be 2
            $state.DeleteCount | Should -Be 3
        }

        It "Exhausts retries and reports error in parallel mode" {
            # Set up state to track calls - fail all delete requests
            $state = [PSCustomObject]@{ FailCount = 0 }
            $interceptor = {
                param($request)
                # Fail all ExecuteMultiple requests that contain delete operations
                if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                    $hasDelete = $false
                    foreach ($req in $request.Requests) {
                        if ($req -is [Microsoft.Xrm.Sdk.Messages.DeleteRequest]) {
                            $hasDelete = $true
                            break
                        }
                    }
                    if ($hasDelete) {
                        $state.FailCount++
                        throw [Exception]::new("Persistent failure")
                    }
                }
            }.GetNewClosure()
            $connection = getMockConnection -RequestInterceptor $interceptor
            
            # Create one record first (should succeed)
            $c1 = @{ firstname = "John1"; lastname = "Doe1" } | Set-DataverseRecord -Connection $connection -TableName contact -PassThru -ErrorAction Continue

            # Try to delete with limited retries (should fail after exhausting retries)
            $errors = @()
            $c1 | Remove-DataverseRecord -Connection $connection -TableName contact -Retries 2 -InitialRetryDelay 1 -MaxDegreeOfParallelism 2 -BatchSize 10 -ErrorVariable errors -ErrorAction SilentlyContinue

            # Should get errors after exhausting retries
            $errors.Count | Should -BeGreaterThan 0
            
            # Verify multiple attempts were made (initial + 2 retries = 3 total)
            $state.FailCount | Should -Be 3
        }
    }
}
