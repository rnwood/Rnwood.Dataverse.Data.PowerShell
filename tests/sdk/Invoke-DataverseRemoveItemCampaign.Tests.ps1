. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRemoveItemCampaign Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RemoveItemCampaign SDK Cmdlet" {

        It "Invoke-DataverseRemoveItemCampaign executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemoveItemCampaignRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RemoveItemCampaign"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RemoveItemCampaignResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRemoveItemCampaign -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RemoveItemCampaign"
        }

    }
}
