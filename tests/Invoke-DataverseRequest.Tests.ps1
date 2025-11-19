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

Describe "Invoke-DataverseRequest - Path validation" {
    BeforeAll {
        $connection = getMockConnection
    }

    It "Rejects path starting with /api/" {
        {
            Invoke-DataverseRequest -Connection $connection -Method Get -Path "/api/data/v9.2/systemusers"
        } | Should -Throw -ExpectedMessage "*should not start with '/api/' or 'api/'*"
    }

    It "Rejects path starting with api/" {
        {
            Invoke-DataverseRequest -Connection $connection -Method Get -Path "api/data/v9.2/systemusers"
        } | Should -Throw -ExpectedMessage "*should not start with '/api/' or 'api/'*"
    }

    It "Rejects path starting with /API/ (case insensitive)" {
        {
            Invoke-DataverseRequest -Connection $connection -Method Get -Path "/API/data/v9.2/systemusers"
        } | Should -Throw -ExpectedMessage "*should not start with '/api/' or 'api/'*"
    }

    It "Rejects path starting with API/ (case insensitive)" {
        {
            Invoke-DataverseRequest -Connection $connection -Method Get -Path "API/data/v9.2/systemusers"
        } | Should -Throw -ExpectedMessage "*should not start with '/api/' or 'api/'*"
    }

    It "Allows navigation path with forward slash for custom API on record" {
        # This is the actual use case from the issue
        $id = "1d936fda-9076-ef11-a671-6045bd0ab99c"
        {
            Invoke-DataverseRequest -Connection $connection -Method POST -Path "sample_entities($id)/Microsoft.Dynamics.CRM.sample_MyCustomApi"
        } | Should -Not -Throw
    }

    It "Allows navigation path with multiple forward slashes" {
        $id = "1d936fda-9076-ef11-a671-6045bd0ab99c"
        {
            Invoke-DataverseRequest -Connection $connection -Method POST -Path "entities($id)/Microsoft.Dynamics.CRM.Action/SubPath"
        } | Should -Not -Throw
    }

    It "Allows simple resource name without slashes" {
        {
            Invoke-DataverseRequest -Connection $connection -Method Get -Path "WhoAmI"
        } | Should -Not -Throw
    }

    It "Allows forward slash in query string" {
        {
            Invoke-DataverseRequest -Connection $connection -Method Get -Path "WhoAmI?filter=value/with/slashes"
        } | Should -Not -Throw
    }

    It "Allows navigation path with query string containing slashes" {
        $id = "1d936fda-9076-ef11-a671-6045bd0ab99c"
        {
            Invoke-DataverseRequest -Connection $connection -Method POST -Path "sample_entities($id)/Microsoft.Dynamics.CRM.Action?param=value/with/slash"
        } | Should -Not -Throw
    }
}


