. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGenerateInvoiceFromOpportunity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GenerateInvoiceFromOpportunity SDK Cmdlet" {

        It "Invoke-DataverseGenerateInvoiceFromOpportunity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GenerateInvoiceFromOpportunityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GenerateInvoiceFromOpportunity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GenerateInvoiceFromOpportunityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGenerateInvoiceFromOpportunity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GenerateInvoiceFromOpportunity"
        }

    }
}
