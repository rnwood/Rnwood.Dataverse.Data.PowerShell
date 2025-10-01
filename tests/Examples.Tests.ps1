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

    Context "Solution Management Examples" {
        It "Can query for solutions" {
            # Create a mock solution record
            $solution = New-Object Microsoft.Xrm.Sdk.Entity("solution")
            $solution.Id = $solution["solutionid"] = [Guid]::NewGuid()
            $solution["uniquename"] = "TestSolution"
            $solution["friendlyname"] = "Test Solution"
            $solution["version"] = "1.0.0.0"
            
            $solution | Set-DataverseRecord -Connection $script:conn
            
            # Query for solutions
            $solutions = Get-DataverseRecord -Connection $script:conn -TableName solution
            $solutions | Should -Not -BeNull
        }
    }

    Context "User and Team Operations Examples" {
        It "Can invoke WhoAmI to get current user" {
            $whoami = Get-DataverseWhoAmI -Connection $script:conn
            $whoami | Should -Not -BeNull
            $whoami.UserId | Should -Not -BeNullOrEmpty
            $whoami.BusinessUnitId | Should -Not -BeNullOrEmpty
            $whoami.OrganizationId | Should -Not -BeNullOrEmpty
        }

        It "Can query system users" {
            # Create a mock user
            $user = New-Object Microsoft.Xrm.Sdk.Entity("systemuser")
            $user.Id = $user["systemuserid"] = [Guid]::NewGuid()
            $user["fullname"] = "Test User"
            $user["internalemailaddress"] = "test@example.com"
            
            $user | Set-DataverseRecord -Connection $script:conn
            
            # Query users
            $users = Get-DataverseRecord -Connection $script:conn -TableName systemuser
            $users | Should -Not -BeNull
        }
    }

    Context "Workflow and Async Job Examples" {
        It "Can query workflow definitions" {
            # Create a mock workflow
            $workflow = New-Object Microsoft.Xrm.Sdk.Entity("workflow")
            $workflow.Id = $workflow["workflowid"] = [Guid]::NewGuid()
            $workflow["name"] = "Test Workflow"
            $workflow["type"] = 1
            
            $workflow | Set-DataverseRecord -Connection $script:conn
            
            # Query workflows
            $workflows = Get-DataverseRecord -Connection $script:conn -TableName workflow
            $workflows | Should -Not -BeNull
        }

        It "Can query async operations" {
            # Create a mock async operation
            $asyncOp = New-Object Microsoft.Xrm.Sdk.Entity("asyncoperation")
            $asyncOp.Id = $asyncOp["asyncoperationid"] = [Guid]::NewGuid()
            $asyncOp["name"] = "Test Operation"
            $asyncOp["operationtype"] = 10
            $asyncOp["statuscode"] = 20
            
            $asyncOp | Set-DataverseRecord -Connection $script:conn
            
            # Query async operations
            $operations = Get-DataverseRecord -Connection $script:conn -TableName asyncoperation
            $operations | Should -Not -BeNull
        }
    }

    Context "Organization Settings Examples" {
        It "Can retrieve organization settings" {
            $whoami = Get-DataverseWhoAmI -Connection $script:conn
            $orgId = $whoami.OrganizationId
            
            # Query organization record
            $org = Get-DataverseRecord -Connection $script:conn -TableName organization -Id $orgId
            $org | Should -Not -BeNull
        }
    }

    Context "Invoke-DataverseRequest Examples" {
        It "Can execute WhoAmI request using Invoke-DataverseRequest" {
            $request = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
            $response = Invoke-DataverseRequest -Connection $script:conn -Request $request
            
            $response | Should -Not -BeNull
            $response.UserId | Should -Not -BeNullOrEmpty
        }

        It "Can execute WhoAmI using RequestName parameter (simpler syntax)" {
            $response = Invoke-DataverseRequest -Connection $script:conn -RequestName "WhoAmI"
            
            $response | Should -Not -BeNull
            $response.UserId | Should -Not -BeNullOrEmpty
        }

        It "Can execute multiple requests" {
            $request1 = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
            $response1 = Invoke-DataverseRequest -Connection $script:conn -Request $request1
            
            $request2 = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
            $response2 = Invoke-DataverseRequest -Connection $script:conn -Request $request2
            
            $response1.UserId | Should -Be $response2.UserId
        }

        It "Can execute SetState request using RequestName and Parameters" {
            # Create a test workflow
            $workflow = New-Object Microsoft.Xrm.Sdk.Entity("workflow")
            $workflowId = $workflow.Id = $workflow["workflowid"] = [Guid]::NewGuid()
            $workflow["name"] = "Test Workflow"
            $workflow | Set-DataverseRecord -Connection $script:conn
            
            # Use simplified syntax to change state
            $response = Invoke-DataverseRequest -Connection $script:conn -RequestName "SetState" -Parameters @{
                EntityMoniker = New-Object Microsoft.Xrm.Sdk.EntityReference("workflow", $workflowId)
                State = New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)
                Status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            }
            
            # Should not throw
            $response | Should -Not -BeNull
        }

        It "Can use AddMemberList request with RequestName syntax" {
            # Create test marketing list and contact
            $list = New-Object Microsoft.Xrm.Sdk.Entity("list")
            $listId = $list.Id = $list["listid"] = [Guid]::NewGuid()
            $list["listname"] = "Test List"
            $list | Set-DataverseRecord -Connection $script:conn
            
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "List"
            $contact["lastname"] = "Member"
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Use simplified syntax
            { Invoke-DataverseRequest -Connection $script:conn -RequestName "AddMemberList" -Parameters @{
                ListId = $listId
                EntityId = $contactId
            } } | Should -Not -Throw
        }

        It "Can use PublishDuplicateRule request with RequestName syntax" {
            # Create a test duplicate rule
            $rule = New-Object Microsoft.Xrm.Sdk.Entity("duplicaterule")
            $ruleId = $rule.Id = $rule["duplicateruleid"] = [Guid]::NewGuid()
            $rule["name"] = "Test Rule"
            $rule | Set-DataverseRecord -Connection $script:conn
            
            # Use simplified syntax
            { Invoke-DataverseRequest -Connection $script:conn -RequestName "PublishDuplicateRule" -Parameters @{
                DuplicateRuleId = $ruleId
            } } | Should -Not -Throw
        }

        It "Can compare verbose vs simplified syntax results" {
            # Verbose syntax
            $request1 = New-Object Microsoft.Crm.Sdk.Messages.WhoAmIRequest
            $response1 = Invoke-DataverseRequest -Connection $script:conn -Request $request1
            
            # Simplified syntax
            $response2 = Invoke-DataverseRequest -Connection $script:conn -RequestName "WhoAmI"
            
            # Both should return same results
            $response1.UserId | Should -Be $response2.UserId
            $response1.OrganizationId | Should -Be $response2.OrganizationId
        }
    }

    Context "Business Process Flow Examples" {
        It "Can query process stages" {
            # Create a mock process stage
            $stage = New-Object Microsoft.Xrm.Sdk.Entity("processstage")
            $stage.Id = $stage["processstageid"] = [Guid]::NewGuid()
            $stage["stagename"] = "Qualify"
            $stage["primaryentitytypecode"] = "lead"
            $stage["stagecategory"] = 0
            
            $stage | Set-DataverseRecord -Connection $script:conn
            
            # Query stages
            $stages = Get-DataverseRecord -Connection $script:conn -TableName processstage
            $stages | Should -Not -BeNull
        }
    }

    Context "Views and Saved Queries Examples" {
        It "Can query saved queries (system views)" {
            # Create a mock saved query
            $view = New-Object Microsoft.Xrm.Sdk.Entity("savedquery")
            $view.Id = $view["savedqueryid"] = [Guid]::NewGuid()
            $view["name"] = "Active Contacts"
            $view["returnedtypecode"] = "contact"
            $view["querytype"] = 0
            
            $view | Set-DataverseRecord -Connection $script:conn
            
            # Query views
            $views = Get-DataverseRecord -Connection $script:conn -TableName savedquery
            $views | Should -Not -BeNull
        }

        It "Can query user queries (personal views)" {
            # Create a mock user query
            $userView = New-Object Microsoft.Xrm.Sdk.Entity("userquery")
            $userView.Id = $userView["userqueryid"] = [Guid]::NewGuid()
            $userView["name"] = "My Contacts"
            $userView["returnedtypecode"] = "contact"
            
            $userView | Set-DataverseRecord -Connection $script:conn
            
            # Query personal views
            $personalViews = Get-DataverseRecord -Connection $script:conn -TableName userquery
            $personalViews | Should -Not -BeNull
        }
    }

    Context "FetchXML Query Examples" {
        It "Can execute FetchXML queries" {
            # Create test data
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "FetchXML"
            $contact["lastname"] = "Test"
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Execute FetchXML
            $fetchXml = @"
<fetch>
  <entity name='contact'>
    <attribute name='firstname' />
    <attribute name='lastname' />
  </entity>
</fetch>
"@
            $results = Get-DataverseRecord -Connection $script:conn -FetchXml $fetchXml
            $results | Should -Not -BeNull
        }

        It "Can execute FetchXML with filters" {
            # Create test data
            $contact1 = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contact1.Id = $contact1["contactid"] = [Guid]::NewGuid()
            $contact1["firstname"] = "Filter"
            $contact1["lastname"] = "Smith"
            $contact1 | Set-DataverseRecord -Connection $script:conn
            
            # Execute FetchXML with filter
            $fetchXml = @"
<fetch>
  <entity name='contact'>
    <attribute name='firstname' />
    <filter>
      <condition attribute='lastname' operator='eq' value='Smith' />
    </filter>
  </entity>
</fetch>
"@
            $results = Get-DataverseRecord -Connection $script:conn -FetchXml $fetchXml
            $results | Should -Not -BeNull
        }
    }

    Context "Record Counting Examples" {
        It "Can count all records in a table" {
            # Create some test data
            3..5 | ForEach-Object {
                $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
                $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
                $contact["firstname"] = "Count$_"
                $contact["lastname"] = "Test"
                $contact | Set-DataverseRecord -Connection $script:conn
            }
            
            # Count records
            $count = Get-DataverseRecord -Connection $script:conn -TableName contact -RecordCount
            $count | Should -BeGreaterThan 0
        }
    }
}
