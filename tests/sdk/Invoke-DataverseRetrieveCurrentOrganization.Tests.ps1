. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveCurrentOrganization Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveCurrentOrganization SDK Cmdlet" {

        It "Invoke-DataverseRetrieveCurrentOrganization executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveCurrentOrganizationRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveCurrentOrganization"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveCurrentOrganizationResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveCurrentOrganization -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveCurrentOrganization"
        }

    }
}
