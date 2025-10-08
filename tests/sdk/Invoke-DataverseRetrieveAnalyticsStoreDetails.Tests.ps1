. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAnalyticsStoreDetails Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAnalyticsStoreDetails SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAnalyticsStoreDetails executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAnalyticsStoreDetailsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAnalyticsStoreDetails"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAnalyticsStoreDetailsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAnalyticsStoreDetails -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAnalyticsStoreDetails"
        }

    }
}
