. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseLockSalesOrderPricing Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "LockSalesOrderPricing SDK Cmdlet" {

        It "Invoke-DataverseLockSalesOrderPricing executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.LockSalesOrderPricingRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "LockSalesOrderPricing"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.LockSalesOrderPricingResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseLockSalesOrderPricing -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "LockSalesOrderPricing"
        }

    }
}
