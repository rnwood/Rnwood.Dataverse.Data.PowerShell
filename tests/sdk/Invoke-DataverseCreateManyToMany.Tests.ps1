. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateManyToMany Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateManyToMany SDK Cmdlet" {

        It "Invoke-DataverseCreateManyToMany executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateManyToManyRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateManyToManyRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateManyToManyResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateManyToMany -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateManyToManyRequest"
        }

    }
}
