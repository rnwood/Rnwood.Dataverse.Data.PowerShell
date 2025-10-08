. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUnlockSalesOrderPricing Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UnlockSalesOrderPricing SDK Cmdlet" {

        It "Invoke-DataverseUnlockSalesOrderPricing executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UnlockSalesOrderPricingRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UnlockSalesOrderPricing"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UnlockSalesOrderPricingResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUnlockSalesOrderPricing -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UnlockSalesOrderPricing"
        }

    }
}
