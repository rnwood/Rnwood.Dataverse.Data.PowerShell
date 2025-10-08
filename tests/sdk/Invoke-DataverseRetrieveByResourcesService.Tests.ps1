. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveByResourcesService Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveByResourcesService SDK Cmdlet" {

        It "Invoke-DataverseRetrieveByResourcesService executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveByResourcesServiceRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveByResourcesService"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveByResourcesServiceResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveByResourcesService -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveByResourcesService"
        }

    }
}
