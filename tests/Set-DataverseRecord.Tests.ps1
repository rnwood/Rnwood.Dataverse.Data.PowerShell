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

    It "-NoCreate prevents new records from being created" {
        $connection = getMockConnection -Entities 'contact'
        $o = [PSCustomObject]@{ firstname = 'NC'; lastname = 'Test' }
        { $o | Set-DataverseRecord -Connection $connection -TableName contact -NoCreate } | Should -Not -Throw

        $found = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ firstname = 'NC'; lastname = 'Test' }
        $found | Should -BeNullOrEmpty
    }

    It "-NoUpdate prevents updating existing records" {
        $connection = getMockConnection -Entities 'contact'
        $id = [Guid]::NewGuid()
        $initial = [PSCustomObject]@{ contactid = $id; firstname = 'NU'; lastname = 'Test'; telephone1 = '111' }
        $initial | Set-DataverseRecord -Connection $connection -TableName contact

        $update = [PSCustomObject]@{ firstname = 'NU'; lastname = 'Test'; telephone1 = '999' }
        $update | Set-DataverseRecord -Connection $connection -TableName contact -MatchOn @(@('firstname','lastname')) -NoUpdate

        $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $id
        $retrieved.telephone1 | Should -Be '111'
    }

    It "-CreateOnly always attempts to create new record" {
        $connection = getMockConnection -Entities 'contact'
        $id = [Guid]::NewGuid()
        $initial = [PSCustomObject]@{ contactid = $id; firstname = 'CO'; lastname = 'Test' }
        $initial | Set-DataverseRecord -Connection $connection -TableName contact

        # Using CreateOnly should attempt to create another record even if one exists
        $dup = [PSCustomObject]@{ firstname = 'CO'; lastname = 'Test' }
        $dup | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly

        $all = Get-DataverseRecord -Connection $connection -TableName contact -FilterValues @{ firstname = 'CO'; lastname = 'Test' }
        $all | Should -HaveCount 2
    }

    It "Upsert is incompatible with MatchOn (throws)" {
        $connection = getMockConnection -Entities 'contact'
        $obj = [PSCustomObject]@{ firstname = 'U1'; lastname = 'U2' }
        { $obj | Set-DataverseRecord -Connection $connection -TableName contact -Upsert -MatchOn @(@('firstname')) } | Should -Throw
    }

    It "Can create records using PSObjects with native PowerShell types" {
        $connection = getMockConnection -Entities 'contact'

        $contact = [PSCustomObject]@{
            contactid = [Guid]::NewGuid()
            firstname = 'PSJohn'
            lastname = 'PSmith'
            birthdate = [datetime]::Parse('1980-01-01T00:00:00Z')
        }

        $contact | Set-DataverseRecord -Connection $connection -TableName contact

        $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $contact.contactid
        $retrieved | Should -Not -BeNull
        $retrieved.firstname | Should -Be 'PSJohn'
        $retrieved.birthdate.Date | Should -Be $contact.birthdate.Date
    }

    It "Can upsert (update) an existing record using a PSObject with the Id set" {
        $connection = getMockConnection -Entities 'contact'

        $id = [Guid]::NewGuid()
        $initial = [PSCustomObject]@{ contactid = $id; firstname = 'Before'; lastname = 'Change' }
        $initial | Set-DataverseRecord -Connection $connection -TableName contact

        $update = [PSCustomObject]@{ contactid = $id; firstname = 'After' }
        $update | Set-DataverseRecord -Connection $connection -TableName contact

        $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $id
        $retrieved.firstname | Should -Be 'After'
        $retrieved.lastname | Should -Be 'Change'
    }

    It "Can create multiple PSObjects in a single pipeline batch" {
        $connection = getMockConnection -Entities 'contact'

        $objs = 1..5 | ForEach-Object {
            [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = "Pipelined$_"; lastname = 'Test' }
        }

        $objs | Set-DataverseRecord -Connection $connection -TableName contact

        $objs | ForEach-Object {
            $r = Get-DataverseRecord -Connection $connection -TableName contact -Id $_.contactid
            $r | Should -Not -BeNull
            $r.firstname | Should -Be $_.firstname
        }
    }

    It "Can set lookup fields by name when supplying a string on PSObject" {
        # Create an account then create a contact referencing it by name
        $connection = getMockConnection -Entities @('account','contact')

        $acctName = "Contoso Testing - " + ([Guid]::NewGuid()).ToString().Substring(0,6)
        $account = [PSCustomObject]@{ accountid = [Guid]::NewGuid(); name = $acctName }
        $account | Set-DataverseRecord -Connection $connection -TableName account

        $contact = [PSCustomObject]@{ contactid = [Guid]::NewGuid(); firstname = 'LookupByName'; parentcustomerid = $acctName }
        $contact | Set-DataverseRecord -Connection $connection -TableName contact

        $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $contact.contactid
        $retrieved.parentcustomerid | Should -BeOfType 'Rnwood.Dataverse.Data.PowerShell.Commands.DataverseEntityReference'
        $retrieved.parentcustomerid.TableName | Should -Be 'account'
        # When Get uses LookupValuesReturnName we should get the name instead
        $retrieved = Get-DataverseRecord -Connection $connection -TableName contact -Id $contact.contactid
        $retrieved.parentcustomerid | Should -Be $account.Id
    }

    It "Accepts option-set labels when setting picklist fields on PSObject" {
        $connection = getMockConnection -Entities 'account'

        # accountcategorycode has options: Preferred Customer (1) and Standard (2)
        $acct = [PSCustomObject]@{ accountid = [Guid]::NewGuid(); name = 'OptTest-' + [Guid]::NewGuid().ToString().Substring(0,6); accountcategorycode = 'Preferred Customer' }
        $acct | Set-DataverseRecord -Connection $connection -TableName account

        $retrieved = Get-DataverseRecord -Connection $connection -TableName account -Id $acct.accountid
        # The converter stores picklist values as int in PS output
        $retrieved.accountcategorycode | Should -BeOfType [int]
        $retrieved.accountcategorycode | Should -Be 1
    }

    It "Can set money fields using PSObject and retrieve numeric value" {
        $connection = getMockConnection -Entities 'opportunity'

        $opp = [PSCustomObject]@{ opportunityid = [Guid]::NewGuid(); name = 'MoneyTest'; estimatedvalue = 12345.67 }
        $opp | Set-DataverseRecord -Connection $connection -TableName opportunity

        $retrieved = Get-DataverseRecord -Connection $connection -TableName opportunity -Id $opp.opportunityid
        $retrieved.estimatedvalue | Should -Be 12345.67
    }
}
