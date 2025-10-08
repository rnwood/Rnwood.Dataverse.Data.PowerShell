. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddMemberList Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddMemberList SDK Cmdlet" {

        It "Invoke-DataverseAddMemberList adds members to a marketing list" {
            $listId = [Guid]::NewGuid()
            $memberId1 = [Guid]::NewGuid()
            $memberId2 = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddMemberListRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddMemberListRequest"
                $request.ListId | Should -BeOfType [System.Guid]
                $request.EntityId | Should -BeOfType [System.Guid]
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.AddMemberListResponse
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseAddMemberList -Connection $script:conn -ListId $listId -EntityId $memberId1
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddMemberListResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.ListId | Should -Be $listId
            $proxy.LastRequest.EntityId | Should -Be $memberId1
        }
    }

    }
}
