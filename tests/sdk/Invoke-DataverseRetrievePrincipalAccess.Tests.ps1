. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseRetrievePrincipalAccess Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "RetrievePrincipalAccess SDK Cmdlet" {
        It "Invoke-DataverseRetrievePrincipalAccess retrieves access rights" {
            $contactId = [Guid]::NewGuid()
            $userId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RetrievePrincipalAccessRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrievePrincipalAccessResponse
                $response.Results["AccessRights"] = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess
                return $response
            })
            
            # Call the cmdlet
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contactId)
            $principal = New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", $userId)
            
            $response = Invoke-DataverseRetrievePrincipalAccess -Connection $script:conn -Target $target -Principal $principal
            
            # Verify response
            $response | Should -Not -BeNull
            $response.AccessRights | Should -Be ([Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess)
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RetrievePrincipalAccessRequest"
        }
    }
}
