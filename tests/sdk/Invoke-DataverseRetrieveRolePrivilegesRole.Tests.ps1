. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveRolePrivilegesRole Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveRolePrivilegesRole SDK Cmdlet" {

        It "Invoke-DataverseRetrieveRolePrivilegesRole executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveRolePrivilegesRoleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveRolePrivilegesRole"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveRolePrivilegesRoleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveRolePrivilegesRole -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveRolePrivilegesRole"
        }

    }
}
