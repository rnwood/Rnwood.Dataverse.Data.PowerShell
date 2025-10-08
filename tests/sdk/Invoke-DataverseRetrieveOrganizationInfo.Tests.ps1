. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveOrganizationInfo Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveOrganizationInfo SDK Cmdlet" {

        It "Invoke-DataverseRetrieveOrganizationInfo executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveOrganizationInfoRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveOrganizationInfo"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveOrganizationInfoResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveOrganizationInfo -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveOrganizationInfo"
        }

    }
}
