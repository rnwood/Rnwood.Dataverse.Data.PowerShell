. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCheckIncomingEmail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CheckIncomingEmail SDK Cmdlet" {

        It "Invoke-DataverseCheckIncomingEmail executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CheckIncomingEmailRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CheckIncomingEmailRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CheckIncomingEmailResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with minimal required parameters
            $response = Invoke-DataverseCheckIncomingEmail -Connection $script:conn -MessageId "test-message-id@example.com" -From "sender@example.com" -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CheckIncomingEmailRequest"
        }

    }
}
