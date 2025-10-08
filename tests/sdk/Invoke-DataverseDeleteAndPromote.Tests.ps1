. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteAndPromote Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteAndPromote SDK Cmdlet" {

        It "Invoke-DataverseDeleteAndPromote executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteAndPromoteRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DeleteAndPromoteRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DeleteAndPromoteResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDeleteAndPromote -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteAndPromoteRequest"
        }

    }
}
