Describe "Remove-DataverseRecord" {
   
    . $PSScriptRoot/Common.ps1

    Context "Basic Removal" {
        It "Removes a single record" {
            $connection = getMockConnection
            $record = @{ firstname = "Test"; lastname = "User" }
            $created = $record | Set-DataverseRecord -Connection $connection -TableName contact -PassThru
            $created.Id | Should -Not -BeNullOrEmpty

            $created | Remove-DataverseRecord -Connection $connection -TableName contact

            $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $created.Id
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

            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -verbose
            $remaining.Count | Should -Be 0
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
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -Retries 1 -initialretry 1 -Verbose

            # Check they are deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -verbose 
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
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -Retries 1  -initialretry 1 -Verbose

            # Check they are deleted
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -verbose
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
            $created | Remove-DataverseRecord -Connection $connection -TableName contact -Retries 1  -initialretry 1 -verbose -ErrorVariable errors -ErrorAction SilentlyContinue

            $errors.Count | Should -Be 2

            # Verify records are still there
            $remaining = Get-DataverseRecord -Connection $connection -TableName contact -verbose
            $remaining.Count | Should -Be 2
        }
    }
}
