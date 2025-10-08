. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseGrantAccess Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "GrantAccess SDK Cmdlet" {
        It "Invoke-DataverseGrantAccess grants access to a record" {
            $contactId = [Guid]::NewGuid()
            $userId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.GrantAccessRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.GrantAccessResponse
                return $response
            })
            
            # Call the cmdlet
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contactId)
            $principalAccess = New-Object Microsoft.Crm.Sdk.Messages.PrincipalAccess
            $principalAccess.Principal = New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", $userId)
            $principalAccess.AccessMask = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess
            
            { Invoke-DataverseGrantAccess -Connection $script:conn -Target $target -PrincipalAccess $principalAccess } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "GrantAccessRequest"
            $proxy.LastRequest.Target.Id | Should -Be $contactId
        }
    }
}
