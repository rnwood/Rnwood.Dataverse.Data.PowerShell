. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUserSetOfPrivilegesByIds Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUserSetOfPrivilegesByIds SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUserSetOfPrivilegesByIds executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUserSetOfPrivilegesByIdsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveUserSetOfPrivilegesByIds"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUserSetOfPrivilegesByIdsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveUserSetOfPrivilegesByIds -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUserSetOfPrivilegesByIds"
        }

    }
}
