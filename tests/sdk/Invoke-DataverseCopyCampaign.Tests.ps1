. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCopyCampaign Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CopyCampaign SDK Cmdlet" {

        It "Invoke-DataverseCopyCampaign executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CopyCampaignRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CopyCampaignRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CopyCampaignResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCopyCampaign -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CopyCampaignRequest"
        }

    }
}
