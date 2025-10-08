. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateStateValue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateStateValue SDK Cmdlet" {

        It "Invoke-DataverseUpdateStateValue executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateStateValueRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateStateValue"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateStateValueResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateStateValue -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateStateValue"
        }

    }
}
