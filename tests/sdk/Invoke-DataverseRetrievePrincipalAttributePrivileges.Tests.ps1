. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrievePrincipalAttributePrivileges Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrievePrincipalAttributePrivileges SDK Cmdlet" {

        It "Invoke-DataverseRetrievePrincipalAttributePrivileges executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrievePrincipalAttributePrivileges"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrievePrincipalAttributePrivilegesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrievePrincipalAttributePrivileges -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrievePrincipalAttributePrivileges"
        }

    }
}
