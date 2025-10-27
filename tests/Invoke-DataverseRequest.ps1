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

    Context "StopProcessing (Ctrl+C) Support" {
        It "InvokeDataverseRequestCmdlet has StopProcessing override" {
            # Verify that the cmdlet class has a StopProcessing method
            $cmdletType = [Rnwood.Dataverse.Data.PowerShell.Commands.InvokeDataverseRequestCmdlet]
            $stopProcessingMethod = $cmdletType.GetMethod("StopProcessing", [System.Reflection.BindingFlags]::Instance -bor [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Public)
            
            $stopProcessingMethod | Should -Not -BeNullOrEmpty
            $stopProcessingMethod.DeclaringType.Name | Should -Be "InvokeDataverseRequestCmdlet"
        }
    }

    Context "Bypass Business Logic Parameters" {
        It "Does not throw when bypass parameters are provided with batching" {
            # Create test requests
            $requests = 1..3 | ForEach-Object { New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest }

            # Execute with bypass parameters and batching - should not throw
            {
                $requests | Invoke-DataverseRequest -Connection $connection `
                    -BypassBusinessLogicExecution CustomSync,CustomAsync `
                    -BypassBusinessLogicExecutionStepIds @([Guid]::NewGuid(), [Guid]::NewGuid()) `
                    -BatchSize 10
            } | Should -Not -Throw
        }

        It "Does not throw when bypass parameters are provided without batching" {
            # Create a single test request
            $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest

            # Execute with bypass parameters but no batching - should not throw
            {
                $request | Invoke-DataverseRequest -Connection $connection `
                    -BypassBusinessLogicExecution CustomAsync `
                    -BypassBusinessLogicExecutionStepIds @([Guid]::NewGuid()) `
                    -BatchSize 1
            } | Should -Not -Throw
        }
    }
}

