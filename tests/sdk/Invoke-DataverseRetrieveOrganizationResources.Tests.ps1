. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveOrganizationResources Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveOrganizationResources SDK Cmdlet" {

        It "Invoke-DataverseRetrieveOrganizationResources retrieves organization resource limits" {
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveOrganizationResourcesRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveOrganizationResourcesRequest"
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveOrganizationResourcesResponse
                $response.Results["MaxSharePointSiteCollections"] = 10
                $response.Results["MaxSharePointStorageSize"] = 1024
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseRetrieveOrganizationResources -Connection $script:conn
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveOrganizationResourcesResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveOrganizationResourcesRequest"
        }
    }

    }
}
