. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseSetStateDynamicEntity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "SetStateDynamicEntity SDK Cmdlet" {

        It "Invoke-DataverseSetStateDynamicEntity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetStateDynamicEntityRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "SetStateDynamicEntityRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.SetStateDynamicEntityResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseSetStateDynamicEntity -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "SetStateDynamicEntityRequest"
        }

    }
}
