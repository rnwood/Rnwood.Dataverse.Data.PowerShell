. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateException Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateException SDK Cmdlet" {

        It "Invoke-DataverseCreateException executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateExceptionRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateExceptionRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateExceptionResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateException -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateExceptionRequest"
        }

    }
}
