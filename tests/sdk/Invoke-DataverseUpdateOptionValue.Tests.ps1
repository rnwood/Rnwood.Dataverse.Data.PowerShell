. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateOptionValue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateOptionValue SDK Cmdlet" {

        It "Invoke-DataverseUpdateOptionValue executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateOptionValueRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateOptionValue"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateOptionValueResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateOptionValue -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateOptionValue"
        }

    }
}
