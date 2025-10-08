. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUserPrivilegeByPrivilegeId Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUserPrivilegeByPrivilegeId SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUserPrivilegeByPrivilegeId executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeIdRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveUserPrivilegeByPrivilegeId"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUserPrivilegeByPrivilegeIdResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveUserPrivilegeByPrivilegeId -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUserPrivilegeByPrivilegeId"
        }

    }
}
