. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateEntityKey Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateEntityKey SDK Cmdlet" {

        It "Invoke-DataverseCreateEntityKey executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateEntityKeyRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateEntityKeyRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateEntityKeyResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateEntityKey -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateEntityKeyRequest"
        }

    }
}
