. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteAttribute Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteAttribute SDK Cmdlet" {

        It "Invoke-DataverseDeleteAttribute executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteAttributeRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DeleteAttributeRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DeleteAttributeResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDeleteAttribute -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteAttributeRequest"
        }

    }
}
