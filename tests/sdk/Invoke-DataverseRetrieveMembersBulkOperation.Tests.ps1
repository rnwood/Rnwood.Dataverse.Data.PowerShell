. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveMembersBulkOperation Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveMembersBulkOperation SDK Cmdlet" {

        It "Invoke-DataverseRetrieveMembersBulkOperation executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveMembersBulkOperationRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveMembersBulkOperation"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveMembersBulkOperationResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveMembersBulkOperation -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveMembersBulkOperation"
        }

    }
}
