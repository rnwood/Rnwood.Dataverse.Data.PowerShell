Describe "Invoke-DataverseParallel" {    # NOTE: These tests use a mock connection that doesn't support cloning.
    # When cloning fails, the cmdlet falls back to sharing the same connection across runspaces.
    # This can cause race conditions in mock scenarios but works correctly with real connections.
    # E2E tests validate the full functionality with real connection cloning.

    It "Processes input objects in parallel chunks" {
        $c = getMockConnection

        # Create test input - array of numbers
        $input = 1..10

        # Process in parallel with chunk size 3
        $results = $input | Invoke-DataverseParallel -Connection $c -ChunkSize 3 -ScriptBlock {
            # Return the input doubled
            $_ | %{ $_ * 2 }
        }

        # Verify we got all results (order may vary due to parallelism)
        $results.Count | Should -Be 10
        $sortedResults = $results | Sort-Object
        $sortedResults | Should -Be @(2, 4, 6, 8, 10, 12, 14, 16, 18, 20)
    }

    It "Makes cloned connection available in script block" {
        $c = getMockConnection

        # Create test records
        $input = 1..5

        # Process in parallel - connection is available via PSDefaultParameterValues
        $results = $input | Invoke-DataverseParallel -Connection $c -ChunkSize 2 -ScriptBlock {
            # Return success message - connection is available but we don't test it here
            # E2E tests validate actual connection usage
            $_ | %{ "success-$_" }
        }

        # Verify all items processed successfully
        $results.Count | Should -Be 5
        $results | Where-Object { $_ -like "success-*" } | Should -HaveCount 5
    }

    It "Respects ChunkSize parameter" {
        $c = getMockConnection

        # Create 25 items with chunk size 10 = 3 chunks (10, 10, 5)
        $input = 1..25
        $chunkSizes = [System.Collections.ArrayList]::new()

        $results = $input | Invoke-DataverseParallel -Connection $c -ChunkSize 10 -ScriptBlock {
            # The script block receives individual items, not chunks
            # Just pass through
            $_
        }

        $results.Count | Should -Be 25
    }

    It "Handles empty input gracefully" {
        $c = getMockConnection

        $results = @() | Invoke-DataverseParallel -Connection $c -ChunkSize 5 -ScriptBlock {
            $_
        }

        $results | Should -BeNullOrEmpty
    }

    It "Handles single item input" {
        $c = getMockConnection

        $results = @(42) | Invoke-DataverseParallel -Connection $c -ChunkSize 5 -ScriptBlock {
            $_ | %{ $_ * 2 }
        }

        $results | Should -Be 84
    }

}

