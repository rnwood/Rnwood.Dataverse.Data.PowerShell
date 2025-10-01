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
        It "Can create a record" {
            $fields = @{
                firstname = "John"
                lastname = "Smith"
            }
            
            $contactId = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields $fields
            $contactId | Should -Not -BeNullOrEmpty
        }

        It "Can update a record" {
            # Create a contact first
            $fields = @{
                firstname = "Jane"
                lastname = "Doe"
            }
            $contactId = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields $fields
            
            # Update the contact
            Set-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId -Fields @{
                telephone1 = "555-5678"
            }
            
            # Verify update
            $contact = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
            $contact.telephone1 | Should -Be "555-5678"
        }

        It "Can delete a record" {
            # Create a contact
            $fields = @{
                firstname = "Delete"
                lastname = "Me"
            }
            $contactId = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields $fields
            
            # Delete it
            Remove-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
            
            # Verify it's gone
            { Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId } | Should -Throw
        }

        It "Can retrieve a single record" {
            # Create a contact
            $fields = @{
                firstname = "Get"
                lastname = "Record"
            }
            $contactId = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields $fields
            
            # Retrieve it
            $contact = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
            $contact | Should -Not -BeNull
            $contact.firstname | Should -Be "Get"
            $contact.lastname | Should -Be "Record"
        }
    }

    Context "Querying Records" {
        BeforeAll {
            # Create some test contacts
            Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Alice"
                lastname = "Smith"
            }
            Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Bob"
                lastname = "Smith"
            }
            Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Charlie"
                lastname = "Jones"
            }
        }

        It "Can retrieve all records of a type" {
            $contacts = Get-DataverseRecord -Connection $script:conn -TableName contact -Columns firstname,lastname
            $contacts.Count | Should -BeGreaterThan 0
        }

        It "Can query with filter" {
            $contacts = Get-DataverseRecord -Connection $script:conn -TableName contact -Filter @{lastname = "Smith"}
            $contacts | Should -Not -BeNull
            $contacts | ForEach-Object {
                $_.lastname | Should -Be "Smith"
            }
        }

        It "Can count records" {
            $count = Get-DataverseRecord -Connection $script:conn -TableName contact -RecordCount
            $count | Should -BeGreaterThan 0
        }

        It "Can use FetchXML for complex queries" {
            $fetchXml = @"
<fetch>
  <entity name='contact'>
    <attribute name='firstname' />
    <attribute name='lastname' />
    <filter>
      <condition attribute='lastname' operator='eq' value='Smith' />
    </filter>
  </entity>
</fetch>
"@
            $contacts = Get-DataverseRecord -Connection $script:conn -FetchXml $fetchXml
            $contacts | Should -Not -BeNull
        }
    }

    Context "Batch Operations" {
        It "Can create multiple records" {
            $contacts = @(
                @{ firstname = "Batch1"; lastname = "Test" }
                @{ firstname = "Batch2"; lastname = "Test" }
                @{ firstname = "Batch3"; lastname = "Test" }
            )

            # Create records using pipeline
            $results = $contacts | Set-DataverseRecord -Connection $script:conn -TableName contact
            
            # Should return array of IDs
            $results.Count | Should -Be 3
        }

        It "Can update multiple records" {
            # Create some records
            $id1 = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Update1"
                lastname = "Test"
            }
            $id2 = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Update2"
                lastname = "Test"
            }

            # Update them
            @(
                @{ Id = $id1; telephone1 = "555-0001" }
                @{ Id = $id2; telephone1 = "555-0002" }
            ) | Set-DataverseRecord -Connection $script:conn -TableName contact

            # Verify updates
            $contact1 = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $id1
            $contact1.telephone1 | Should -Be "555-0001"
            $contact2 = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $id2
            $contact2.telephone1 | Should -Be "555-0002"
        }

        It "Can delete multiple records" {
            # Create some records
            $id1 = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Delete1"
                lastname = "Test"
            }
            $id2 = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Delete2"
                lastname = "Test"
            }

            # Delete them
            @($id1, $id2) | Remove-DataverseRecord -Connection $script:conn -TableName contact

            # Verify deletion
            { Get-DataverseRecord -Connection $script:conn -TableName contact -Id $id1 } | Should -Throw
            { Get-DataverseRecord -Connection $script:conn -TableName contact -Id $id2 } | Should -Throw
        }
    }

    Context "Lookup and Type Conversion" {
        It "Can use lookup by name" {
            # This tests that the module can resolve lookups by name
            # Create a parent contact
            $parentId = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Parent"
                lastname = "Contact"
            }

            # Create a child contact with parentcustomerid lookup
            # The module should automatically resolve the name to an ID
            $childId = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Child"
                lastname = "Contact"
                parentcustomerid = "Parent Contact"
            }

            $childId | Should -Not -BeNullOrEmpty
        }

        It "Can use choice/optionset values by label" {
            # Create a contact with a choice field using the label
            $contactId = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Choice"
                lastname = "Test"
                gendercode = "Male"  # Using label instead of numeric value
            }

            $contactId | Should -Not -BeNullOrEmpty
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
            # Create a contact
            $fields = @{
                firstname = "Specific"
                lastname = "Columns"
                emailaddress1 = "test@example.com"
                telephone1 = "555-1234"
            }
            $contactId = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields $fields
            
            # Retrieve only specific columns
            $contact = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId -Columns firstname,lastname
            
            $contact.firstname | Should -Be "Specific"
            $contact.lastname | Should -Be "Columns"
            # Note: Depending on implementation, other fields might be present or not
        }

        It "Can retrieve all columns" {
            # Create a contact
            $fields = @{
                firstname = "All"
                lastname = "Columns"
            }
            $contactId = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields $fields
            
            # Retrieve all columns (default behavior)
            $contact = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $contactId
            
            $contact.firstname | Should -Be "All"
            $contact.lastname | Should -Be "Columns"
        }
    }

    Context "Upsert Operations" {
        It "Can upsert records (create if not exists, update if exists)" {
            # First upsert - should create
            $result1 = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Upsert"
                lastname = "Test"
                emailaddress1 = "upsert@example.com"
            } -MatchOn emailaddress1
            
            $result1 | Should -Not -BeNullOrEmpty
            
            # Second upsert with same email - should update
            $result2 = Set-DataverseRecord -Connection $script:conn -TableName contact -Fields @{
                firstname = "Updated"
                lastname = "Test"
                emailaddress1 = "upsert@example.com"
                telephone1 = "555-9999"
            } -MatchOn emailaddress1
            
            # Should be the same ID
            $result2 | Should -Be $result1
            
            # Verify update occurred
            $contact = Get-DataverseRecord -Connection $script:conn -TableName contact -Id $result2
            $contact.firstname | Should -Be "Updated"
            $contact.telephone1 | Should -Be "555-9999"
        }
    }
}
