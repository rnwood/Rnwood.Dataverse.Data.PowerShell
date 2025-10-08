. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateRibbonClientMetadata Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateRibbonClientMetadata SDK Cmdlet" {

        It "Invoke-DataverseUpdateRibbonClientMetadata executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateRibbonClientMetadataRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "UpdateRibbonClientMetadata"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.UpdateRibbonClientMetadataResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseUpdateRibbonClientMetadata -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateRibbonClientMetadata"
        }

    }
}
