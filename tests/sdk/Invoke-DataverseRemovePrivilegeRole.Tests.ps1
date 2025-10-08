. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRemovePrivilegeRole Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RemovePrivilegeRole SDK Cmdlet" {

        It "Invoke-DataverseRemovePrivilegeRole executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemovePrivilegeRoleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RemovePrivilegeRole"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RemovePrivilegeRoleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRemovePrivilegeRole -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RemovePrivilegeRole"
        }

    }
}
