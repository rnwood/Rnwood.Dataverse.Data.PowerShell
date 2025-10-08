. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRemoveItemCampaignActivity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RemoveItemCampaignActivity SDK Cmdlet" {

        It "Invoke-DataverseRemoveItemCampaignActivity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemoveItemCampaignActivityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RemoveItemCampaignActivity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RemoveItemCampaignActivityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRemoveItemCampaignActivity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RemoveItemCampaignActivity"
        }

    }
}
