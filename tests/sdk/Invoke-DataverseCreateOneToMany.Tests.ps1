. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateOneToMany Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateOneToMany SDK Cmdlet" {

        It "Invoke-DataverseCreateOneToMany executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateOneToManyRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateOneToManyRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateOneToManyResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateOneToMany -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateOneToManyRequest"
        }

    }
}
