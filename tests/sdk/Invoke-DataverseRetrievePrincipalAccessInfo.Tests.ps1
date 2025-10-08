. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrievePrincipalAccessInfo Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrievePrincipalAccessInfo SDK Cmdlet" {

        It "Invoke-DataverseRetrievePrincipalAccessInfo executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrievePrincipalAccessInfoRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrievePrincipalAccessInfo"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrievePrincipalAccessInfoResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrievePrincipalAccessInfo -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrievePrincipalAccessInfo"
        }

    }
}
