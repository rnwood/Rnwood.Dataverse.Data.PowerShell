. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseExpandCalendar Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ExpandCalendar SDK Cmdlet" {

        It "Invoke-DataverseExpandCalendar executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExpandCalendarRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "ExpandCalendar"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.ExpandCalendarResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseExpandCalendar -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ExpandCalendar"
        }

    }
}
