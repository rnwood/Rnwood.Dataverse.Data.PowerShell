. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseWhoAmI Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "WhoAmIRequest SDK Cmdlet" {

        It "Invoke-DataverseWhoAmI executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.WhoAmIRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "WhoAmIRequest"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.WhoAmIResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseWhoAmI -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "WhoAmIRequest"
        }

    }
}
