. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddUserToRecordTeam Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddUserToRecordTeam SDK Cmdlet" {
        It "Invoke-DataverseAddUserToRecordTeam adds user to record team" {
            $recordId = [Guid]::NewGuid()
            $userId = [Guid]::NewGuid()
            $teamTemplateId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddUserToRecordTeamRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddUserToRecordTeamRequest"
                $request.Record | Should -Not -BeNull
                $request.Record | Should -BeOfType [Microsoft.Xrm.Sdk.EntityReference]
                $request.SystemUserId | Should -BeOfType [System.Guid]
                $request.TeamTemplateId | Should -BeOfType [System.Guid]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.AddUserToRecordTeamResponse
                $accessTeamId = [Guid]::NewGuid()
                $response.Results["AccessTeamId"] = $accessTeamId
                return $response
            })
            
            # Call the cmdlet
            $record = New-Object Microsoft.Xrm.Sdk.EntityReference("account", $recordId)
            $response = Invoke-DataverseAddUserToRecordTeam -Connection $script:conn -Record $record -SystemUserId $userId -TeamTemplateId $teamTemplateId
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddUserToRecordTeamResponse"
            $response.AccessTeamId | Should -Not -BeNullOrEmpty
            $response.AccessTeamId | Should -BeOfType [System.Guid]
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.Record.Id | Should -Be $recordId
            $proxy.LastRequest.SystemUserId | Should -Be $userId
            $proxy.LastRequest.TeamTemplateId | Should -Be $teamTemplateId
        }
    }
}
