. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRemoveMembersTeam Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RemoveMembersTeam SDK Cmdlet" {

        It "Invoke-DataverseRemoveMembersTeam executes successfully" {
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemoveMembersTeamRequest", {
                param($request)
                
                $request.GetType().FullName | Should -Match "RemoveMembersTeam"
                
                $responseType = "Microsoft.Crm.Sdk.Messages.RemoveMembersTeamResponse" -as [Type]
                if ($responseType) {
                    $response = New-Object $responseType
                } else {
                    $response = New-Object Microsoft.Xrm.Sdk.OrganizationResponse
                }
                return $response
            })
            
            $response = Invoke-DataverseRemoveMembersTeam -Connection $script:conn -Confirm:$false
            
            $response | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().FullName | Should -Match "RemoveMembersTeam"
        }

    }
}
