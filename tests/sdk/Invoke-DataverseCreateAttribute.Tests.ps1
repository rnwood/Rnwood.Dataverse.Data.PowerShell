. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateAttribute Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateAttribute SDK Cmdlet" {

        It "Invoke-DataverseCreateAttribute executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateAttributeRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateAttributeRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateAttributeResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateAttribute -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateAttributeRequest"
        }

    }
}
