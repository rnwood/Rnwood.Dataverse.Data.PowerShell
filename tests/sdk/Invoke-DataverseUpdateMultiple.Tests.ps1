. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseUpdateMultiple Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "UpdateMultiple SDK Cmdlet" {

        It "Invoke-DataverseUpdateMultiple executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.UpdateMultipleRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "UpdateMultipleRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.UpdateMultipleResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseUpdateMultiple -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "UpdateMultipleRequest"
        }

    }
}
