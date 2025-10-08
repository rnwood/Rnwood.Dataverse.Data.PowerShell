. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseCreateInstance Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "CreateInstance SDK Cmdlet" {

        It "Invoke-DataverseCreateInstance executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CreateInstanceRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "CreateInstanceRequest"
                
                # Create response
                $response = New-Object ("Microsoft.Crm.Sdk.Messages.CreateInstanceResponse" -as [Type])
                if (-not $response) {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseCreateInstance -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "CreateInstanceRequest"
        }

    }
}
