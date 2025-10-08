. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateFeatureConfig Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateFeatureConfig SDK Cmdlet" {

        It "Invoke-DataverseUpdateFeatureConfig executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateFeatureConfigRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateFeatureConfig"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateFeatureConfigResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateFeatureConfig -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateFeatureConfig"
        }

    }
}
