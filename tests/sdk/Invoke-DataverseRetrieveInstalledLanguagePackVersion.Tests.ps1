. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveInstalledLanguagePackVersion Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveInstalledLanguagePackVersion SDK Cmdlet" {

        It "Invoke-DataverseRetrieveInstalledLanguagePackVersion executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveInstalledLanguagePackVersionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveInstalledLanguagePackVersion"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveInstalledLanguagePackVersionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveInstalledLanguagePackVersion -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveInstalledLanguagePackVersion"
        }

    }
}
