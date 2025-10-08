. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseBook Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Book SDK Cmdlet" {

        It "Invoke-DataverseBook executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.BookRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "BookRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.BookResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseBook -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "BookRequest"
        }

    }
}
