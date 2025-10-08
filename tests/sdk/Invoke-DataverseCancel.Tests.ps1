. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCancel Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Cancel SDK Cmdlet" {

        It "Invoke-DataverseCancel executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CancelRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CancelRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CancelResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Create a test target (appointment, etc.)
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference("appointment", [Guid]::NewGuid())
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCancel -Connection $script:conn -Target $target -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CancelRequest"
        }

    }
}
