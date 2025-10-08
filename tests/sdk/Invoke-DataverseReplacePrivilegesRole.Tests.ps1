. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseReplacePrivilegesRole Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "ReplacePrivilegesRole SDK Cmdlet" {

        It "Invoke-DataverseReplacePrivilegesRole executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ReplacePrivilegesRoleRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "ReplacePrivilegesRole"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.ReplacePrivilegesRoleResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseReplacePrivilegesRole -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "ReplacePrivilegesRole"
        }

    }
}
