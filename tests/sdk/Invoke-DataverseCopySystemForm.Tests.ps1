. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCopySystemForm Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CopySystemForm SDK Cmdlet" {

        It "Invoke-DataverseCopySystemForm executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CopySystemFormRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CopySystemFormRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CopySystemFormResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCopySystemForm -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CopySystemFormRequest"
        }

    }
}
