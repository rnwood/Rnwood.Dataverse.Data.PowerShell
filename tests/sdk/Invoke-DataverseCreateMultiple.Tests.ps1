. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateMultiple Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateMultiple SDK Cmdlet" {

        It "Invoke-DataverseCreateMultiple executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateMultipleRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateMultipleRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateMultipleResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateMultiple -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateMultipleRequest"
        }

    }
}
