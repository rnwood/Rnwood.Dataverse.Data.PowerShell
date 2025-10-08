. $PSScriptRoot/../Common.ps1

Describe "Invoke-DataverseAssign Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Assign SDK Cmdlet" {
        It "Invoke-DataverseAssign assigns a record to a user" {
            # Create a contact record first
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Test"
            $contact["lastname"] = "User"
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Create a systemuser ID (doesn't need to exist in fake service)
            $userId = [Guid]::NewGuid()
            
            # Call the Assign cmdlet
            $targetRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contactId)
            $assigneeRef = New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", $userId)
            
            { Invoke-DataverseAssign -Connection $script:conn -Target $targetRef -Assignee $assigneeRef } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "AssignRequest"
            $proxy.LastRequest.Target.LogicalName | Should -Be "contact"
            $proxy.LastRequest.Target.Id | Should -Be $contactId
            $proxy.LastRequest.Assignee.LogicalName | Should -Be "systemuser"
            $proxy.LastRequest.Assignee.Id | Should -Be $userId
        }
    }
}
