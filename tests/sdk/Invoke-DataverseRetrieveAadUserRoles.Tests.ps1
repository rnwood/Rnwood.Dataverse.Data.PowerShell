. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveAadUserRoles Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveAadUserRoles SDK Cmdlet" {

        It "Invoke-DataverseRetrieveAadUserRoles executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveAadUserRolesRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveAadUserRoles"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveAadUserRolesResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveAadUserRoles -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveAadUserRoles"
        }

    }
}
