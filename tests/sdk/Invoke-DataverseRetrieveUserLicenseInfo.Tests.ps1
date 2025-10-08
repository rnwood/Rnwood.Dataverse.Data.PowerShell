. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUserLicenseInfo Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUserLicenseInfo SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUserLicenseInfo executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUserLicenseInfoRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveUserLicenseInfo"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUserLicenseInfoResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveUserLicenseInfo -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUserLicenseInfo"
        }

    }
}
