. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGetInvoiceProductsFromOpportunity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GetInvoiceProductsFromOpportunity SDK Cmdlet" {

        It "Invoke-DataverseGetInvoiceProductsFromOpportunity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GetInvoiceProductsFromOpportunityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GetInvoiceProductsFromOpportunity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GetInvoiceProductsFromOpportunityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGetInvoiceProductsFromOpportunity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GetInvoiceProductsFromOpportunity"
        }

    }
}
