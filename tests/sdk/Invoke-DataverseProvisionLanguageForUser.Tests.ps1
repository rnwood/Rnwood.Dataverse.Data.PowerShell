. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseProvisionLanguageForUser Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ProvisionLanguageForUser SDK Cmdlet" {

        It "Invoke-DataverseProvisionLanguageForUser executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ProvisionLanguageForUserRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ProvisionLanguageForUser"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ProvisionLanguageForUserResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseProvisionLanguageForUser -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ProvisionLanguageForUser"
        }

    }
}
