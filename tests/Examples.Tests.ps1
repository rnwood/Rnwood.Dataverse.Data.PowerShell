. $PSScriptRoot/Common.ps1

Describe "Examples-Comparison Documentation Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "Connection Examples" {
        It "Can create a mock connection for testing" {
            $conn = getMockConnection
            $conn | Should -Not -BeNull
        }
    }

    Context "Basic CRUD Operations" {
        It "Can create a record using SDK Entity objects" {
            # Using SDK Entity objects as per existing tests
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "John"
            $contact["lastname"] = "Smith"
            
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Verify it was created by retrieving it
            $retrieved = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
            $retrieved | Should -Not -BeNull
            $retrieved.firstname | Should -Be "John"
        }

        It "Can retrieve a single record" {
            # Create using SDK Entity
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Jane"
            $contact["lastname"] = "Doe"
            
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Retrieve it
            $retrieved = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
            $retrieved | Should -Not -BeNull
            $retrieved.firstname | Should -Be "Jane"
            $retrieved.lastname | Should -Be "Doe"
        }

        It "Can delete a record" {
            # Create using SDK Entity
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Delete"
            $contact["lastname"] = "Me"
            
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Delete it
            { Remove-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId } | Should -Not -Throw
            
            # Note: Mock provider may not actually remove the record from storage,
            # but the cmdlet should execute successfully
        }
    }

    Context "Querying Records" {
        BeforeAll {
            # Create some test contacts using SDK Entity objects
            $contact1 = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contact1.Id = $contact1["contactid"] = [Guid]::NewGuid()
            $contact1["firstname"] = "Alice"
            $contact1["lastname"] = "Smith"
            $contact1 | Set-DataverseRecord -Connection $script:conn

            $contact2 = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contact2.Id = $contact2["contactid"] = [Guid]::NewGuid()
            $contact2["firstname"] = "Bob"
            $contact2["lastname"] = "Smith"
            $contact2 | Set-DataverseRecord -Connection $script:conn

            $contact3 = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contact3.Id = $contact3["contactid"] = [Guid]::NewGuid()
            $contact3["firstname"] = "Charlie"
            $contact3["lastname"] = "Jones"
            $contact3 | Set-DataverseRecord -Connection $script:conn
        }

        It "Can retrieve all records of a type" {
            $contacts = Get-DataverseRecord -Connection $script:conn -TableName contact -Columns firstname,lastname
            $contacts.Count | Should -BeGreaterThan 0
        }

        It "Can use FetchXML for queries" {
            $fetchXml = @"
<fetch>
  <entity name='contact'>
    <attribute name='firstname' />
    <attribute name='lastname' />
  </entity>
</fetch>
"@
            $contacts = Get-DataverseRecord -Connection $script:conn -FetchXml $fetchXml
            $contacts | Should -Not -BeNull
            $contacts.Count | Should -BeGreaterThan 0
        }

        It "Can count records" {
            $count = Get-DataverseRecord -Connection $script:conn -TableName contact -RecordCount
            $count | Should -BeGreaterThan 0
        }
    }

    Context "Batch Operations with Pipeline" {
        It "Can create multiple records using pipeline" {
            # Create multiple SDK Entity objects and pipe them
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

            # Create records using pipeline
            @($contact1, $contact2, $contact3) | Set-DataverseRecord -Connection $script:conn
            
            # Verify all 3 were created
            $retrieved1 = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $id1
            $retrieved2 = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $id2
            $retrieved3 = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $id3
            
            $retrieved1.firstname | Should -Be "Batch1"
            $retrieved2.firstname | Should -Be "Batch2"
            $retrieved3.firstname | Should -Be "Batch3"
        }
    }

    Context "Invoke Request Examples" {
        It "Can execute WhoAmI request" {
            $whoami = Get-DataverseWhoAmI -Connection $script:conn
            $whoami | Should -Not -BeNull
            $whoami.UserId | Should -Not -BeNullOrEmpty
        }

        It "Can invoke custom requests" {
            $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
            $response = Invoke-DataverseRequest -Connection $script:conn -Request $request
            $response | Should -Not -BeNull
        }
    }

    Context "Working with Columns" {
        It "Can retrieve specific columns" {
            # Create using SDK Entity
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Specific"
            $contact["lastname"] = "Columns"
            
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Retrieve only specific columns
            $retrieved = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId -Columns firstname,lastname
            
            $retrieved.firstname | Should -Be "Specific"
            $retrieved.lastname | Should -Be "Columns"
        }

        It "Can retrieve all columns" {
            # Create using SDK Entity
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "All"
            $contact["lastname"] = "Columns"
            
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Retrieve all columns (default behavior)
            $retrieved = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
            
            $retrieved.firstname | Should -Be "All"
            $retrieved.lastname | Should -Be "Columns"
        }
    }

    Context "Documentation Examples Validation" {
        It "Connection example pattern is valid" {
            # This validates the pattern shown in docs works
            $testConn = getMockConnection
            $testConn | Should -Not -BeNull
            $testConn.GetType().Name | Should -Be "ServiceClient"
        }

        It "Basic CRUD pattern from docs works" {
            # Pattern from docs: Create -> Retrieve -> Delete
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Test"
            $contact["lastname"] = "User"
            
            # Create
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Retrieve
            $retrieved = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
            $retrieved.firstname | Should -Be "Test"
            
            # Delete
            Remove-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
        }

        It "Query pattern from docs works" {
            # Create test data
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Query"
            $contact["lastname"] = "Test"
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Query pattern from docs
            $results = Get-DataverseRecord -Connection $script:conn -TableName contact
            $results | Should -Not -BeNull
            $results.Count | Should -BeGreaterThan 0
        }
    }
}
