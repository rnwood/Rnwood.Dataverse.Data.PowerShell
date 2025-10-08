. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDistributeCampaignActivity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DistributeCampaignActivity SDK Cmdlet" {

        It "Invoke-DataverseDistributeCampaignActivity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DistributeCampaignActivityRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DistributeCampaignActivityRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DistributeCampaignActivityResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDistributeCampaignActivity -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DistributeCampaignActivityRequest"
        }

    }
}
