. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRevokeAccess Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RevokeAccess SDK Cmdlet" {
        It "Invoke-DataverseRevokeAccess revokes access to a record" {
            $contactId = [Guid]::NewGuid()
            $userId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RevokeAccessRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.RevokeAccessResponse
                return $response
            })
            
            # Call the cmdlet
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contactId)
            $revokee = New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", $userId)
            
            { Invoke-DataverseRevokeAccess -Connection $script:conn -Target $target -Revokee $revokee } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RevokeAccessRequest"
            $proxy.LastRequest.Target.Id | Should -Be $contactId
            $proxy.LastRequest.Revokee.Id | Should -Be $userId
        }
    }
}
