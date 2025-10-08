. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddPrivilegesRole Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddPrivilegesRole SDK Cmdlet" {
        It "Invoke-DataverseAddPrivilegesRole adds privileges to a role" {
            $roleId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddPrivilegesRoleRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.AddPrivilegesRoleResponse
                return $response
            })
            
            # Call the cmdlet
            $privilege = New-Object Microsoft.Crm.Sdk.Messages.RolePrivilege
            $privilege.PrivilegeId = [Guid]::NewGuid()
            $privilege.Depth = [Microsoft.Crm.Sdk.Messages.PrivilegeDepth]::Basic
            
            { Invoke-DataverseAddPrivilegesRole -Connection $script:conn -RoleId $roleId -Privileges @($privilege) } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "AddPrivilegesRoleRequest"
            $proxy.LastRequest.RoleId | Should -Be $roleId
        }
    }
}
