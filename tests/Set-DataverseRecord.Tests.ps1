. $PSScriptRoot/Common.ps1

Describe "Set-DataverseRecord examples" {

    It "Can create a record using SDK Entity objects" {
    $connection = getMockConnection -Entities 'contact'

        $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
        $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
        $contact["firstname"] = "John"
        $contact["lastname"] = "Smith"

        $contact | Set-DataverseRecord -Connection $connection

        # Verify it was created by retrieving it
        $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $contactId
        $retrieved | Should -Not -BeNull
        $retrieved.firstname | Should -Be "John"
    }

    It "Can create multiple records using pipeline" {
    $connection = getMockConnection -Entities 'contact'

        $contact1 = New-Object Microsoft.Xrm.Sdk.Entity("contact")
        $id1 = $contact1.Id = $contact1["contactid"] = [Guid]::NewGuid()
        $contact1["firstname"] = "Batch1"
        $contact1["lastname"] = "Test"

        $contact2 = New-Object Microsoft.Xrm.Sdk.Entity("contact")
        $id2 = $contact2.Id = $contact2["contactid"] = [Guid]::NewGuid()
        $contact2["firstname"] = "Batch2"
        $contact2["lastname"] = "Test"

        $contact3 = New-Object Microsoft.Xrm.Sdk.Entity("contact")
        $id3 = $contact3.Id = $contact3["contactid"] = [Guid]::NewGuid()
        $contact3["firstname"] = "Batch3"
        $contact3["lastname"] = "Test"

        @($contact1, $contact2, $contact3) | Set-DataverseRecord -Connection $connection

        # Verify all 3 were created
        $retrieved1 = Get-DataverseRecord -Connection $connection -TableName contact -Id $id1
        $retrieved2 = Get-DataverseRecord -Connection $connection -TableName contact -Id $id2
        $retrieved3 = Get-DataverseRecord -Connection $connection -TableName contact -Id $id3

        $retrieved1.firstname | Should -Be "Batch1"
        $retrieved2.firstname | Should -Be "Batch2"
        $retrieved3.firstname | Should -Be "Batch3"
    }
}
