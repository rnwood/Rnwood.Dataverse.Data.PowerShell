. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUnlockInvoicePricing Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UnlockInvoicePricing SDK Cmdlet" {

        It "Invoke-DataverseUnlockInvoicePricing executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UnlockInvoicePricingRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UnlockInvoicePricing"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UnlockInvoicePricingResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUnlockInvoicePricing -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UnlockInvoicePricing"
        }

    }
}
