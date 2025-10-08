. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUserPrivilegeByPrivilegeName Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUserPrivilegeByPrivilegeName SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUserPrivilegeByPrivilegeName executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeNameRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveUserPrivilegeByPrivilegeName"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeNameResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveUserPrivilegeByPrivilegeName -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUserPrivilegeByPrivilegeName"
        }

    }
}
