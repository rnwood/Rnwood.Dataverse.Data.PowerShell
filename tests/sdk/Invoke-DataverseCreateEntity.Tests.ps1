. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateEntity Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateEntity SDK Cmdlet" {

        It "Invoke-DataverseCreateEntity executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateEntityRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateEntityRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateEntityResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateEntity -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateEntityRequest"
        }

    }
}
