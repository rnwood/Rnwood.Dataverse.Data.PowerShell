. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCloneAsSolution Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CloneAsSolution SDK Cmdlet" {

        It "Invoke-DataverseCloneAsSolution executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CloneAsSolutionRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CloneAsSolutionRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CloneAsSolutionResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCloneAsSolution -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CloneAsSolutionRequest"
        }

    }
}
