. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrieveUsersPrivilegesThroughTeams Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrieveUsersPrivilegesThroughTeams SDK Cmdlet" {

        It "Invoke-DataverseRetrieveUsersPrivilegesThroughTeams executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrieveUsersPrivilegesThroughTeamsRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RetrieveUsersPrivilegesThroughTeams"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RetrieveUsersPrivilegesThroughTeamsResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRetrieveUsersPrivilegesThroughTeams -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RetrieveUsersPrivilegesThroughTeams"
        }

    }
}
