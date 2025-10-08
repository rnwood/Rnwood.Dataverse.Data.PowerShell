. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateUserSettingsSystemUser Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateUserSettingsSystemUser SDK Cmdlet" {

        It "Invoke-DataverseUpdateUserSettingsSystemUser executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateUserSettingsSystemUserRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateUserSettingsSystemUser"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateUserSettingsSystemUserResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateUserSettingsSystemUser -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateUserSettingsSystemUser"
        }

    }
}
