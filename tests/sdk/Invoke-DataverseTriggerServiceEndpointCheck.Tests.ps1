. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseTriggerServiceEndpointCheck Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "TriggerServiceEndpointCheck SDK Cmdlet" {

        It "Invoke-DataverseTriggerServiceEndpointCheck executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.TriggerServiceEndpointCheckRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "TriggerServiceEndpointCheck"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.TriggerServiceEndpointCheckResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseTriggerServiceEndpointCheck -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "TriggerServiceEndpointCheck"
        }

    }
}
