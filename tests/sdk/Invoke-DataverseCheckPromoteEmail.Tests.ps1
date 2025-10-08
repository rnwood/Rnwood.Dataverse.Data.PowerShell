. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCheckPromoteEmail Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CheckPromoteEmail SDK Cmdlet" {

        It "Invoke-DataverseCheckPromoteEmail executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CheckPromoteEmailRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CheckPromoteEmailRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CheckPromoteEmailResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCheckPromoteEmail -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CheckPromoteEmailRequest"
        }

    }
}
