. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseFulfillSalesOrder Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "FulfillSalesOrder SDK Cmdlet" {

        It "Invoke-DataverseFulfillSalesOrder executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.FulfillSalesOrderRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "FulfillSalesOrder"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.FulfillSalesOrderResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseFulfillSalesOrder -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "FulfillSalesOrder"
        }

    }
}
