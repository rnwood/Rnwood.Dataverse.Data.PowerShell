. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveByGroupResource Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveByGroupResource SDK Cmdlet" {

        It "Invoke-DataverseRetrieveByGroupResource executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveByGroupResourceRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveByGroupResource"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveByGroupResourceResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveByGroupResource -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveByGroupResource"
        }

    }
}
