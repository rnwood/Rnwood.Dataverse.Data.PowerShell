. $PSScriptRoot/Common.ps1

Describe "Invoke-DataverseRequest - Batching and responses" {
    BeforeAll {
        $connection = getMockConnection
    }

    It "Batches multiple SDK requests with ExecuteMultiple and returns responses" {
        # Create 10 WhoAmI requests
        $requests = 1..10 | ForEach-Object { New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest }

        # Invoke with BatchSize 5 to force batching
        $results = $requests | Invoke-DataverseRequest -Connection $connection -BatchSize 5

        # Expect one response per request
        $results | Should -Not -BeNullOrEmpty
        $results.Count | Should -Be 10
    }

    It "Handles BatchSize 1 (no batching) and returns responses" {
        $requests = 1..8 | ForEach-Object { New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest }

        $results = $requests | Invoke-DataverseRequest -Connection $connection -BatchSize 1

        $results | Should -Not -BeNullOrEmpty
        $results.Count | Should -Be 8
    }

    It "Does not throw when pipeline is empty" {
        { @() | Invoke-DataverseRequest -Connection $connection -BatchSize 5 } | Should -Not -Throw
    }

}

