. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteOptionValue Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteOptionValue SDK Cmdlet" {

        It "Invoke-DataverseDeleteOptionValue executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteOptionValueRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DeleteOptionValueRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DeleteOptionValueResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDeleteOptionValue -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteOptionValueRequest"
        }

    }
}
