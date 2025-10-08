. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseQueueUpdateRibbonClientMetadata Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "QueueUpdateRibbonClientMetadata SDK Cmdlet" {

        It "Invoke-DataverseQueueUpdateRibbonClientMetadata executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.QueueUpdateRibbonClientMetadataRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "QueueUpdateRibbonClientMetadata"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.QueueUpdateRibbonClientMetadataResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseQueueUpdateRibbonClientMetadata -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "QueueUpdateRibbonClientMetadata"
        }

    }
}
