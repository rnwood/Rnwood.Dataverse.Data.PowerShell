. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteMultiple Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteMultiple SDK Cmdlet" {

        It "Invoke-DataverseDeleteMultiple executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteMultipleRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DeleteMultipleRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DeleteMultipleResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDeleteMultiple -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteMultipleRequest"
        }

    }
}
