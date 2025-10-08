. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseWinQuote Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "WinQuote SDK Cmdlet" {

        It "Invoke-DataverseWinQuote executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.WinQuoteRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "WinQuote"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.WinQuoteResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseWinQuote -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "WinQuote"
        }

    }
}
