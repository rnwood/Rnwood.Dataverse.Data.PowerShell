. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCalculateActualValueOpportunity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CalculateActualValueOpportunity SDK Cmdlet" {

        It "Invoke-DataverseCalculateActualValueOpportunity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CalculateActualValueOpportunityRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CalculateActualValueOpportunityRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CalculateActualValueOpportunityResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCalculateActualValueOpportunity -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CalculateActualValueOpportunityRequest"
        }

    }
}
