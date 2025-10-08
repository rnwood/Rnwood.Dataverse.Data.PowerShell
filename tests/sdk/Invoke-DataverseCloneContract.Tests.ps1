. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCloneContract Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CloneContract SDK Cmdlet" {

        It "Invoke-DataverseCloneContract executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CloneContractRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CloneContractRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CloneContractResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCloneContract -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CloneContractRequest"
        }

    }
}
