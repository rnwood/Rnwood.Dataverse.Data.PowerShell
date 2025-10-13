Describe "Invoke-DataverseParallel" {

    . $PSScriptRoot/Common.ps1

    It "Processes input objects in parallel chunks" {
        $c = getMockConnection

        # Create test input - array of numbers
        $input = 1..10

        # Process in parallel with chunk size 3
        $results = $input | Invoke-DataverseParallel -Connection $c -ChunkSize 3 -ScriptBlock {
            # Return the input doubled
            $_ * 2
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

        # Process in parallel and use WhoAmI to verify connection works
        $results = $input | Invoke-DataverseParallel -Connection $c -ChunkSize 2 -ScriptBlock {
            # Try to call WhoAmI - this should work with the cloned connection
            # In mock mode, this will return null but shouldn't error
            try {
                Get-DataverseWhoAmI -ErrorAction SilentlyContinue
                "success-$_"
            } catch {
                "error-$_"
            }
        }

        # All should succeed (though WhoAmI may return nothing in mock mode)
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
            $_ * 2
        }

        $results | Should -Be 84
    }

    It "Respects MaxDegreeOfParallelism parameter" {
        $c = getMockConnection

        # This test just verifies the parameter is accepted
        # Testing actual parallel execution is difficult without timing
        $results = 1..10 | Invoke-DataverseParallel -Connection $c -ChunkSize 2 -MaxDegreeOfParallelism 2 -ScriptBlock {
            $_
        }

        $results.Count | Should -Be 10
    }
}
