. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCancelContract Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CancelContract SDK Cmdlet" {

        It "Invoke-DataverseCancelContract executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CancelContractRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CancelContractRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CancelContractResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCancelContract -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CancelContractRequest"
        }

    }
}
