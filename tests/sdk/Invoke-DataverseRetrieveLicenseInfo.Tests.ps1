. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveLicenseInfo Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveLicenseInfoRequest SDK Cmdlet" {

        It "Invoke-DataverseRetrieveLicenseInfo executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveLicenseInfoRequest", {
                param($request)
                
                # Validate request type
                $request.GetType().FullName | Should -Match "RetrieveLicenseInfoRequest"
                
                # Create response
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveLicenseInfoResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            # Call cmdlet with -Confirm:$false to avoid prompts
            $response = Invoke-DataverseRetrieveLicenseInfo -Connection $script:conn -Confirm:$false
            
            # Verify response
            $response | Should -Not -BeNull
            
            # Verify request via proxy
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveLicenseInfoRequest"
        }

    }
}
