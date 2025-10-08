. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveVersionedPluginInfo Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveVersionedPluginInfo SDK Cmdlet" {

        It "Invoke-DataverseRetrieveVersionedPluginInfo executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveVersionedPluginInfoRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveVersionedPluginInfo"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveVersionedPluginInfoResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveVersionedPluginInfo -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveVersionedPluginInfo"
        }

    }
}
