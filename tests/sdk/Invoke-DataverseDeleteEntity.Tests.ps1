. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteEntity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteEntity SDK Cmdlet" {

        It "Invoke-DataverseDeleteEntity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteEntityRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DeleteEntityRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DeleteEntityResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDeleteEntity -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteEntityRequest"
        }

    }
}
