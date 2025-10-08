. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrievePrincipalSyncAttributeMappings Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrievePrincipalSyncAttributeMappings SDK Cmdlet" {

        It "Invoke-DataverseRetrievePrincipalSyncAttributeMappings executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrievePrincipalSyncAttributeMappingsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrievePrincipalSyncAttributeMappings"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrievePrincipalSyncAttributeMappingsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrievePrincipalSyncAttributeMappings -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrievePrincipalSyncAttributeMappings"
        }

    }
}
