. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteRelationship Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteRelationship SDK Cmdlet" {

        It "Invoke-DataverseDeleteRelationship executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteRelationshipRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DeleteRelationshipRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DeleteRelationshipResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDeleteRelationship -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteRelationshipRequest"
        }

    }
}
