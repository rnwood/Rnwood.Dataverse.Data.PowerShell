. $PSScriptRoot/Common.ps1

Describe "Remove-DataverseRecord examples" {

    It "Can delete a record" {
    $connection = getMockConnection -Entities 'contact'

        $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
        $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
        $contact["firstname"] = "Delete"
        $contact["lastname"] = "Me"

        $contact | Set-DataverseRecord -Connection $connection

        { Remove-DataverseRecord -Connection $connection -TableName contact -Id $contactId } | Should -Not -Throw
    }
}
