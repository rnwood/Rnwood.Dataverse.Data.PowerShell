. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetSalesOrderProductsFromOpportunity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetSalesOrderProductsFromOpportunity SDK Cmdlet" {

        It "Invoke-DataverseGetSalesOrderProductsFromOpportunity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetSalesOrderProductsFromOpportunityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetSalesOrderProductsFromOpportunity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetSalesOrderProductsFromOpportunityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetSalesOrderProductsFromOpportunity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetSalesOrderProductsFromOpportunity"
        }

    }
}
