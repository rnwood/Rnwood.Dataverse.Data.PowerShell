. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveProvisionedLanguagePackVersion Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveProvisionedLanguagePackVersion SDK Cmdlet" {

        It "Invoke-DataverseRetrieveProvisionedLanguagePackVersion executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagePackVersionRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveProvisionedLanguagePackVersion"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveProvisionedLanguagePackVersionResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveProvisionedLanguagePackVersion -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveProvisionedLanguagePackVersion"
        }

    }
}
