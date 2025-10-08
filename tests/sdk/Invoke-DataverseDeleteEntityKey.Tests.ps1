. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseDeleteEntityKey Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "DeleteEntityKey SDK Cmdlet" {

        It "Invoke-DataverseDeleteEntityKey executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.DeleteEntityKeyRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "DeleteEntityKeyRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.DeleteEntityKeyResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseDeleteEntityKey -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "DeleteEntityKeyRequest"
        }

    }
}
