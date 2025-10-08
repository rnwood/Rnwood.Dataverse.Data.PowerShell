. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGenerateQuoteFromOpportunity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GenerateQuoteFromOpportunity SDK Cmdlet" {

        It "Invoke-DataverseGenerateQuoteFromOpportunity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GenerateQuoteFromOpportunityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GenerateQuoteFromOpportunity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GenerateQuoteFromOpportunityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGenerateQuoteFromOpportunity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GenerateQuoteFromOpportunity"
        }

    }
}
