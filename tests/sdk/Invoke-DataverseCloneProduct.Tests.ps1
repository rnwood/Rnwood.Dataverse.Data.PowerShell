. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCloneProduct Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CloneProduct SDK Cmdlet" {

        It "Invoke-DataverseCloneProduct executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CloneProductRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CloneProductRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CloneProductResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCloneProduct -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CloneProductRequest"
        }

    }
}
