. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddItemCampaignActivity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddItemCampaignActivity SDK Cmdlet" {

        It "Invoke-DataverseAddItemCampaignActivity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddItemCampaignActivityRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "AddItemCampaignActivity"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.AddItemCampaignActivityResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseAddItemCampaignActivity -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "AddItemCampaignActivity"
        }

    }
}
