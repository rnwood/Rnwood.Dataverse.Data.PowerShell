. "$PSScriptRoot/Common.ps1"

Describe "Bypass Business Logic Parameters" {
    BeforeAll {
        $connection = getMockConnection
    }

    Context "Invoke-DataverseRequest with bypass parameters" {
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

    Context "Set-DataverseRecord with bypass parameters" {
        It "Does not throw when bypass parameters are provided with batching" {
            # Create test records
            $records = 1..3 | ForEach-Object {
                [PSCustomObject]@{
                    firstname = "Test$_"
                    lastname = "User$_"
                }
            }

            # Execute with bypass parameters and batching - should not throw
            {
                $records | Set-DataverseRecord -Connection $connection -TableName contact `
                    -BypassBusinessLogicExecution CustomSync,CustomAsync `
                    -BypassBusinessLogicExecutionStepIds @([Guid]::NewGuid()) `
                    -BatchSize 10 -CreateOnly
            } | Should -Not -Throw
        }
    }

    Context "Remove-DataverseRecord with bypass parameters" {
        It "Does not throw when bypass parameters are provided with batching" {
            # Create test records first so we can delete them
            $records = 1..3 | ForEach-Object {
                [PSCustomObject]@{
                    firstname = "BypassTest$_"
                    lastname = "ToDelete$_"
                }
            }
            
            $createdIds = $records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru | 
                ForEach-Object { $_.Id }

            # Execute with bypass parameters and batching - should not throw
            {
                $createdIds | ForEach-Object {
                    Remove-DataverseRecord -Connection $connection -TableName contact -Id $_ `
                        -BypassBusinessLogicExecution CustomSync `
                        -BypassBusinessLogicExecutionStepIds @([Guid]::NewGuid()) `
                        -BatchSize 10
                }
            } | Should -Not -Throw
        }
    }
}
