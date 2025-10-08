. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveBusinessHierarchyBusinessUnit Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveBusinessHierarchyBusinessUnit SDK Cmdlet" {

        It "Invoke-DataverseRetrieveBusinessHierarchyBusinessUnit executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveBusinessHierarchyBusinessUnitRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveBusinessHierarchyBusinessUnit"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveBusinessHierarchyBusinessUnitResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveBusinessHierarchyBusinessUnit -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveBusinessHierarchyBusinessUnit"
        }

    }
}
