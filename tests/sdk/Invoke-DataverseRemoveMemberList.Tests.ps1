. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRemoveMemberList Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RemoveMemberList SDK Cmdlet" {
        It "Invoke-DataverseRemoveMemberList removes a member from a list" {
            $listId = [Guid]::NewGuid()
            $entityId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemoveMemberListRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.RemoveMemberListResponse
                return $response
            })
            
            # Call the cmdlet
            { Invoke-DataverseRemoveMemberList -Connection $script:conn -ListId $listId -EntityId $entityId } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RemoveMemberListRequest"
            $proxy.LastRequest.ListId | Should -Be $listId
            $proxy.LastRequest.EntityId | Should -Be $entityId
        }
    }
}
