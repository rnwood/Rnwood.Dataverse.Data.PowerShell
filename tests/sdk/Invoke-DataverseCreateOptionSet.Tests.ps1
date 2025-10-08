. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateOptionSet Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateOptionSet SDK Cmdlet" {

        It "Invoke-DataverseCreateOptionSet executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateOptionSetRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateOptionSetRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateOptionSetResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateOptionSet -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateOptionSetRequest"
        }

    }
}
