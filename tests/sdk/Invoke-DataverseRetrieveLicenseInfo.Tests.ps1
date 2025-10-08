. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveLicenseInfo Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveLicenseInfo SDK Cmdlet" {
        It "Invoke-DataverseRetrieveLicenseInfo retrieves license information" {
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveLicenseInfoRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveLicenseInfoRequest"
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveLicenseInfoResponse
                $response.Results["AvailableCount"] = 100
                $response.Results["GrantedLicenseCount"] = 50
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseRetrieveLicenseInfo -Connection $script:conn
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveLicenseInfoResponse"
            $response.AvailableCount | Should -Be 100
            $response.GrantedLicenseCount | Should -Be 50
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.RetrieveLicenseInfoRequest"
        }
    }
}
