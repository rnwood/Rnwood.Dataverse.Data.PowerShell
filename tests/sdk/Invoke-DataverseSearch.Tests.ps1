. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSearch Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Search SDK Cmdlet" {

        It "Invoke-DataverseSearch executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SearchRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "Search"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.SearchResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseSearch -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "Search"
        }

    }
}
