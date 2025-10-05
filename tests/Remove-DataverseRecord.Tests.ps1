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

    It "Can delete multiple records piped from Get-DataverseRecord" {
        $connection = getMockConnection -Entities 'contact'

        # Create 3 contacts
        1..3 | ForEach-Object { [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = "Del$_" } } | Set-DataverseRecord -Connection $connection -TableName contact

        # Fetch them and pipe to Remove; this uses the TableName/Id properties returned by Get-DataverseRecord
        Get-DataverseRecord -Connection $connection -TableName contact -filter @{ firstname = 'Del1' } | Remove-DataverseRecord -Connection $connection

        # Verify the specific one was deleted, others remain
        $remaining = Get-DataverseRecord -Connection $connection -TableName contact
        $remaining.firstname | Should -Not -Contain 'Del1'
    }

    It "WhatIf does not delete records" {
        $connection = getMockConnection -Entities 'contact'
        $c = [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = 'WhatIfTest' }
        $c | Set-DataverseRecord -Connection $connection -TableName contact

        # Using -WhatIf should not perform deletion
        { Remove-DataverseRecord -Connection $connection -TableName contact -Id $c.contactid -WhatIf } | Should -Not -Throw

        # Record should still exist
        $exists = Get-DataverseRecord -Connection $connection -TableName contact -Id $c.contactid
        $exists | Should -Not -BeNull
    }

    It "Deletes records in batches when BatchSize is specified (pipeline)" {
        $connection = getMockConnection -Entities 'contact'
        $records = 1..10 | ForEach-Object { [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = "BatchDel$_" } }
        $records | Set-DataverseRecord -Connection $connection -TableName contact

        # Remove via pipeline using a small batch size
        Get-DataverseRecord -Connection $connection -TableName contact | Where-Object { $_.firstname -like 'BatchDel*' } | Remove-DataverseRecord -Connection $connection -BatchSize 3

        # Verify none remain
        $left = Get-DataverseRecord -Connection $connection -TableName contact | Where-Object { $_.firstname -like 'BatchDel*' }
        $left | Should -BeNullOrEmpty
    }

    # Many-to-many intersect tests are not added here because the required intersect metadata
    # files are not guaranteed to be present in the test metadata set. Add targeted intersect
    # tests when the appropriate intersect entity metadata is included in tests/metadata/.
}
