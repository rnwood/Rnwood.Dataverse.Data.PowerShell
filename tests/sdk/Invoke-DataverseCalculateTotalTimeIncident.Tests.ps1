. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCalculateTotalTimeIncident Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CalculateTotalTimeIncident SDK Cmdlet" {

        It "Invoke-DataverseCalculateTotalTimeIncident executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CalculateTotalTimeIncidentRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CalculateTotalTimeIncidentRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CalculateTotalTimeIncidentResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCalculateTotalTimeIncident -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CalculateTotalTimeIncidentRequest"
        }

    }
}
