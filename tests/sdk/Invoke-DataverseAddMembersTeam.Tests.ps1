. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddMembersTeam Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddMembersTeam SDK Cmdlet" {
        It "Invoke-DataverseAddMembersTeam adds users to a team" {
            $teamId = [Guid]::NewGuid()
            $userId1 = [Guid]::NewGuid()
            $userId2 = [Guid]::NewGuid()
            
            # Stub the response since we just want to test the request format
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddMembersTeamRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.AddMembersTeamResponse
                return $response
            })
            
            # Call the cmdlet
            $memberIds = @($userId1, $userId2)
            { Invoke-DataverseAddMembersTeam -Connection $script:conn -TeamId $teamId -MemberIds $memberIds } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "AddMembersTeamRequest"
            $proxy.LastRequest.TeamId | Should -Be $teamId
            $proxy.LastRequest.MemberIds.Count | Should -Be 2
            $proxy.LastRequest.MemberIds[0] | Should -Be $userId1
            $proxy.LastRequest.MemberIds[1] | Should -Be $userId2
        }
    }
}
