. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGrantAccessUsingSharedLink Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GrantAccessUsingSharedLink SDK Cmdlet" {

        It "Invoke-DataverseGrantAccessUsingSharedLink executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GrantAccessUsingSharedLinkRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "GrantAccessUsingSharedLink"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.GrantAccessUsingSharedLinkResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseGrantAccessUsingSharedLink -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "GrantAccessUsingSharedLink"
        }

    }
}
