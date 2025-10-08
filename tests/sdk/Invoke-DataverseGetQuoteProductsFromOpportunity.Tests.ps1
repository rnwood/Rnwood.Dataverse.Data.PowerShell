. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetQuoteProductsFromOpportunity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetQuoteProductsFromOpportunity SDK Cmdlet" {

        It "Invoke-DataverseGetQuoteProductsFromOpportunity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetQuoteProductsFromOpportunityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetQuoteProductsFromOpportunity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetQuoteProductsFromOpportunityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetQuoteProductsFromOpportunity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetQuoteProductsFromOpportunity"
        }

    }
}
