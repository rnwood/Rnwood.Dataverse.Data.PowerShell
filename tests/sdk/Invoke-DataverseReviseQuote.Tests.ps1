. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseReviseQuote Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ReviseQuote SDK Cmdlet" {

        It "Invoke-DataverseReviseQuote executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ReviseQuoteRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ReviseQuote"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ReviseQuoteResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseReviseQuote -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ReviseQuote"
        }

    }
}
