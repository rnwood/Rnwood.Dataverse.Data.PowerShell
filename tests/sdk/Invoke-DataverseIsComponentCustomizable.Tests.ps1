. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseIsComponentCustomizable Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "IsComponentCustomizable SDK Cmdlet" {

        It "Invoke-DataverseIsComponentCustomizable executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.IsComponentCustomizableRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "IsComponentCustomizable"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.IsComponentCustomizableResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseIsComponentCustomizable -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "IsComponentCustomizable"
        }

    }
}
