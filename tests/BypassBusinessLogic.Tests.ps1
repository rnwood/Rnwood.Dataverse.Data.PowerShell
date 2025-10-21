. "$PSScriptRoot/Common.ps1"

Describe "Bypass Business Logic Parameters" {
    BeforeAll {
        # Import the cmdlet module to get access to types
        Import-Module "$env:TESTMODULEPATH/Rnwood.Dataverse.Data.PowerShell.psd1" -Force
    }

    Context "Set-DataverseRecord with bypass parameters" {
        It "Applies bypass parameters to individual requests and batch request" {
            # Create a mock connection that tracks requests
            $executedRequests = @()
            $mockService = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.MockOrganizationServiceWithScriptBlock -ArgumentList @{
                "Execute" = {
                    param($request)
                    $executedRequests += $request
                    
                    # Return appropriate response based on request type
                    if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.ExecuteMultipleResponse
                        $response.Results["Responses"] = New-Object Microsoft.Xrm.Sdk.ExecuteMultipleResponseItemCollection
                        
                        # Add a response for each request in the batch
                        for ($i = 0; $i -lt $request.Requests.Count; $i++) {
                            $item = New-Object Microsoft.Xrm.Sdk.ExecuteMultipleResponseItem
                            $item.RequestIndex = $i
                            
                            # Create appropriate response based on request type
                            if ($request.Requests[$i] -is [Microsoft.Xrm.Sdk.Messages.CreateRequest]) {
                                $createResponse = New-Object Microsoft.Xrm.Sdk.Messages.CreateResponse
                                $createResponse.Results["id"] = [Guid]::NewGuid()
                                $item.Response = $createResponse
                            } elseif ($request.Requests[$i] -is [Microsoft.Xrm.Sdk.Messages.UpdateRequest]) {
                                $item.Response = New-Object Microsoft.Xrm.Sdk.Messages.UpdateResponse
                            }
                            
                            $response.Results["Responses"].Add($item)
                        }
                        
                        return $response
                    } elseif ($request -is [Microsoft.Crm.Sdk.Messages.RetrieveMetadataChangesRequest]) {
                        $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveMetadataChangesResponse
                        $entityMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
                        $entityMetadata.LogicalName = "contact"
                        $entityMetadata.PrimaryIdAttribute = "contactid"
                        $entityMetadata.IsIntersect = $false
                        
                        # Add minimal attributes
                        $idAttr = New-Object Microsoft.Xrm.Sdk.Metadata.AttributeMetadata
                        $idAttr.LogicalName = "contactid"
                        $entityMetadata.SetAttribute("Attributes", @($idAttr))
                        
                        $response.Results["EntityMetadata"] = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadataCollection
                        $response.Results["EntityMetadata"].Add($entityMetadata)
                        return $response
                    } elseif ($request -is [Microsoft.Xrm.Sdk.Messages.RetrieveMultipleRequest]) {
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                        $collection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                        $response.Results["EntityCollection"] = $collection
                        return $response
                    }
                    
                    return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
            }
            $connection = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ThreadSafeOrganizationServiceProxy($mockService)

            # Create test records
            $records = 1..3 | ForEach-Object {
                [PSCustomObject]@{
                    firstname = "Test$_"
                    lastname = "User$_"
                }
            }

            # Execute with bypass parameters and batching
            $records | Set-DataverseRecord -Connection $connection -TableName contact `
                -BypassBusinessLogicExecution CustomSync,CustomAsync `
                -BypassBusinessLogicExecutionStepIds @([Guid]::NewGuid()) `
                -BatchSize 10 -CreateOnly

            # Verify ExecuteMultipleRequest was called
            $batchRequests = $executedRequests | Where-Object { $_ -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest] }
            $batchRequests.Count | Should -Be 1
            
            $batchRequest = $batchRequests[0]
            
            # Verify bypass parameters are set on the batch request
            $batchRequest.Parameters.ContainsKey("BypassBusinessLogicExecution") | Should -Be $true
            $batchRequest.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomSync"
            $batchRequest.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomAsync"
            $batchRequest.Parameters.ContainsKey("BypassBusinessLogicExecutionStepIds") | Should -Be $true
            
            # Verify bypass parameters are set on individual requests in the batch
            foreach ($request in $batchRequest.Requests) {
                if ($request -is [Microsoft.Xrm.Sdk.Messages.CreateRequest]) {
                    $request.Parameters.ContainsKey("BypassBusinessLogicExecution") | Should -Be $true
                    $request.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomSync"
                    $request.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomAsync"
                    $request.Parameters.ContainsKey("BypassBusinessLogicExecutionStepIds") | Should -Be $true
                }
            }
        }
    }

    Context "Remove-DataverseRecord with bypass parameters" {
        It "Applies bypass parameters to individual requests and batch request" {
            $executedRequests = @()
            $mockService = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.MockOrganizationServiceWithScriptBlock -ArgumentList @{
                "Execute" = {
                    param($request)
                    $executedRequests += $request
                    
                    if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.ExecuteMultipleResponse
                        $response.Results["Responses"] = New-Object Microsoft.Xrm.Sdk.ExecuteMultipleResponseItemCollection
                        
                        for ($i = 0; $i -lt $request.Requests.Count; $i++) {
                            $item = New-Object Microsoft.Xrm.Sdk.ExecuteMultipleResponseItem
                            $item.RequestIndex = $i
                            $item.Response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteResponse
                            $response.Results["Responses"].Add($item)
                        }
                        
                        return $response
                    } elseif ($request -is [Microsoft.Crm.Sdk.Messages.RetrieveMetadataChangesRequest]) {
                        $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveMetadataChangesResponse
                        $entityMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
                        $entityMetadata.LogicalName = "contact"
                        $entityMetadata.PrimaryIdAttribute = "contactid"
                        $entityMetadata.IsIntersect = $false
                        $response.Results["EntityMetadata"] = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadataCollection
                        $response.Results["EntityMetadata"].Add($entityMetadata)
                        return $response
                    }
                    
                    return New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
            }
            $connection = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ThreadSafeOrganizationServiceProxy($mockService)

            # Create test records to delete
            $ids = 1..3 | ForEach-Object { [Guid]::NewGuid() }

            # Execute with bypass parameters and batching
            $ids | ForEach-Object {
                Remove-DataverseRecord -Connection $connection -TableName contact -Id $_ `
                    -BypassBusinessLogicExecution CustomSync `
                    -BypassBusinessLogicExecutionStepIds @([Guid]::NewGuid()) `
                    -BatchSize 10
            }

            # Verify ExecuteMultipleRequest was called
            $batchRequests = $executedRequests | Where-Object { $_ -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest] }
            $batchRequests.Count | Should -Be 1
            
            $batchRequest = $batchRequests[0]
            
            # Verify bypass parameters are set on the batch request
            $batchRequest.Parameters.ContainsKey("BypassBusinessLogicExecution") | Should -Be $true
            $batchRequest.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomSync"
            $batchRequest.Parameters.ContainsKey("BypassBusinessLogicExecutionStepIds") | Should -Be $true
            
            # Verify bypass parameters are set on individual requests in the batch
            foreach ($request in $batchRequest.Requests) {
                if ($request -is [Microsoft.Xrm.Sdk.Messages.DeleteRequest]) {
                    $request.Parameters.ContainsKey("BypassBusinessLogicExecution") | Should -Be $true
                    $request.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomSync"
                    $request.Parameters.ContainsKey("BypassBusinessLogicExecutionStepIds") | Should -Be $true
                }
            }
        }
    }

    Context "Invoke-DataverseRequest with bypass parameters" {
        It "Applies bypass parameters to individual requests and batch request" {
            $executedRequests = @()
            $mockService = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.MockOrganizationServiceWithScriptBlock -ArgumentList @{
                "Execute" = {
                    param($request)
                    $executedRequests += $request
                    
                    if ($request -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest]) {
                        $response = New-Object Microsoft.Xrm.Sdk.Messages.ExecuteMultipleResponse
                        $response.Results["Responses"] = New-Object Microsoft.Xrm.Sdk.ExecuteMultipleResponseItemCollection
                        
                        for ($i = 0; $i -lt $request.Requests.Count; $i++) {
                            $item = New-Object Microsoft.Xrm.Sdk.ExecuteMultipleResponseItem
                            $item.RequestIndex = $i
                            $whoAmIResponse = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIResponse
                            $whoAmIResponse.Results["UserId"] = [Guid]::NewGuid()
                            $item.Response = $whoAmIResponse
                            $response.Results["Responses"].Add($item)
                        }
                        
                        return $response
                    }
                    
                    # For non-batch WhoAmI requests
                    $whoAmIResponse = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIResponse
                    $whoAmIResponse.Results["UserId"] = [Guid]::NewGuid()
                    return $whoAmIResponse
                }
            }
            $connection = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ThreadSafeOrganizationServiceProxy($mockService)

            # Create test requests
            $requests = 1..3 | ForEach-Object { New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest }

            # Execute with bypass parameters and batching
            $requests | Invoke-DataverseRequest -Connection $connection `
                -BypassBusinessLogicExecution CustomSync,CustomAsync `
                -BypassBusinessLogicExecutionStepIds @([Guid]::NewGuid(), [Guid]::NewGuid()) `
                -BatchSize 10

            # Verify ExecuteMultipleRequest was called
            $batchRequests = $executedRequests | Where-Object { $_ -is [Microsoft.Xrm.Sdk.Messages.ExecuteMultipleRequest] }
            $batchRequests.Count | Should -Be 1
            
            $batchRequest = $batchRequests[0]
            
            # Verify bypass parameters are set on the batch request
            $batchRequest.Parameters.ContainsKey("BypassBusinessLogicExecution") | Should -Be $true
            $batchRequest.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomSync"
            $batchRequest.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomAsync"
            $batchRequest.Parameters.ContainsKey("BypassBusinessLogicExecutionStepIds") | Should -Be $true
            
            # Verify bypass parameters are set on individual requests in the batch
            foreach ($request in $batchRequest.Requests) {
                if ($request -is [Microsoft.Crm.Sdk.Messages.WhoAmIRequest]) {
                    $request.Parameters.ContainsKey("BypassBusinessLogicExecution") | Should -Be $true
                    $request.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomSync"
                    $request.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomAsync"
                    $request.Parameters.ContainsKey("BypassBusinessLogicExecutionStepIds") | Should -Be $true
                }
            }
        }

        It "Applies bypass parameters when not batching (BatchSize 1)" {
            $executedRequests = @()
            $mockService = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.MockOrganizationServiceWithScriptBlock -ArgumentList @{
                "Execute" = {
                    param($request)
                    $executedRequests += $request
                    
                    $whoAmIResponse = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIResponse
                    $whoAmIResponse.Results["UserId"] = [Guid]::NewGuid()
                    return $whoAmIResponse
                }
            }
            $connection = New-Object Rnwood.Dataverse.Data.PowerShell.Commands.ThreadSafeOrganizationServiceProxy($mockService)

            # Create a single test request
            $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest

            # Execute with bypass parameters but no batching
            $request | Invoke-DataverseRequest -Connection $connection `
                -BypassBusinessLogicExecution CustomAsync `
                -BypassBusinessLogicExecutionStepIds @([Guid]::NewGuid()) `
                -BatchSize 1

            # Verify the individual request has bypass parameters
            $executedRequests.Count | Should -Be 1
            $executedRequest = $executedRequests[0]
            
            $executedRequest.Parameters.ContainsKey("BypassBusinessLogicExecution") | Should -Be $true
            $executedRequest.Parameters["BypassBusinessLogicExecution"] | Should -Match "CustomAsync"
            $executedRequest.Parameters.ContainsKey("BypassBusinessLogicExecutionStepIds") | Should -Be $true
        }
    }
}
