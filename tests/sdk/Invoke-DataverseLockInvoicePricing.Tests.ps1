. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseLockInvoicePricing Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "LockInvoicePricing SDK Cmdlet" {

        It "Invoke-DataverseLockInvoicePricing executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.LockInvoicePricingRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "LockInvoicePricing"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.LockInvoicePricingResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseLockInvoicePricing -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "LockInvoicePricing"
        }

    }
}
