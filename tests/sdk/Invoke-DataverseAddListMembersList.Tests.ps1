. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAddListMembersList Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "AddListMembersList SDK Cmdlet" {
        It "Invoke-DataverseAddListMembersList adds multiple members to a marketing list" {
            $listId = [Guid]::NewGuid()
            $memberId1 = [Guid]::NewGuid()
            $memberId2 = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddListMembersListRequest", {
                param($request)
                
                # Validate request parameters
                $request | Should -Not -BeNull
                $request.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddListMembersListRequest"
                $request.ListId | Should -BeOfType [System.Guid]
                $request.MemberIds | Should -Not -BeNull
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.AddListMembersListResponse
                return $response
            })
            
            # Call the cmdlet
            $memberIds = @($memberId1, $memberId2)
            $response = Invoke-DataverseAddListMembersList -Connection $script:conn -ListId $listId -MemberIds $memberIds
            
            # Verify response type
            $response | Should -Not -BeNull
            $response.GetType().FullName | Should -Be "Microsoft.Crm.Sdk.Messages.AddListMembersListResponse"
            
            # Verify request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.ListId | Should -Be $listId
        }
    }
}
