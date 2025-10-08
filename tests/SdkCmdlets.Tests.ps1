. $PSScriptRoot/Common.ps1

Describe "SDK Cmdlet Tests" {

    BeforeAll {
        $script:conn = getMockConnection
    }

    Context "WhoAmI SDK Cmdlet" {
        It "Invoke-DataverseWhoAmI returns valid response" {
            # Call the SDK cmdlet
            $response = Invoke-DataverseWhoAmI -Connection $script:conn
            
            # Verify response structure
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "WhoAmIResponse"
            $response.UserId | Should -Not -BeNullOrEmpty
            $response.BusinessUnitId | Should -Not -BeNullOrEmpty
            $response.OrganizationId | Should -Not -BeNullOrEmpty
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "WhoAmIRequest"
            $proxy.LastResponse | Should -Be $response
        }
    }

    Context "RetrieveVersion SDK Cmdlet" {
        It "Invoke-DataverseRetrieveVersion returns version information" {
            # Call the SDK cmdlet
            $response = Invoke-DataverseRetrieveVersion -Connection $script:conn
            
            # Verify response structure
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "RetrieveVersionResponse"
            $response.Version | Should -Not -BeNullOrEmpty
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RetrieveVersionRequest"
        }
    }

    Context "RetrieveAllEntities SDK Cmdlet" {
        It "Invoke-DataverseRetrieveAllEntities returns entity metadata" {
            # Stub the response since FakeXrmEasy OSS doesn't support this
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesRequest", {
                param($request)
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveAllEntitiesResponse
                
                # Create minimal entity metadata array
                $entityMetadataList = New-Object 'System.Collections.Generic.List[Microsoft.Xrm.Sdk.Metadata.EntityMetadata]'
                
                # Add contact entity metadata
                $contactMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
                $contactMetadata.GetType().GetProperty("LogicalName").SetValue($contactMetadata, "contact")
                $contactMetadata.GetType().GetProperty("SchemaName").SetValue($contactMetadata, "Contact")
                $entityMetadataList.Add($contactMetadata)
                
                # Add account entity metadata
                $accountMetadata = New-Object Microsoft.Xrm.Sdk.Metadata.EntityMetadata
                $accountMetadata.GetType().GetProperty("LogicalName").SetValue($accountMetadata, "account")
                $accountMetadata.GetType().GetProperty("SchemaName").SetValue($accountMetadata, "Account")
                $entityMetadataList.Add($accountMetadata)
                
                $response.Results["EntityMetadata"] = $entityMetadataList.ToArray()
                return $response
            })
            
            # Call the SDK cmdlet
            $response = Invoke-DataverseRetrieveAllEntities -Connection $script:conn -EntityFilters Entity -RetrieveAsIfPublished $true
            
            # Verify response structure
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "RetrieveAllEntitiesResponse"
            $response.EntityMetadata | Should -Not -BeNull
            $response.EntityMetadata.Count | Should -BeGreaterThan 0
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RetrieveAllEntitiesRequest"
            $proxy.LastRequest.EntityFilters | Should -Be "Entity"
            $proxy.LastRequest.RetrieveAsIfPublished | Should -Be $true
        }
    }

    Context "RetrieveEntity SDK Cmdlet" {
        It "Invoke-DataverseRetrieveEntity retrieves contact entity metadata" {
            # Call the SDK cmdlet with contact entity
            $response = Invoke-DataverseRetrieveEntity -Connection $script:conn -LogicalName "contact" -EntityFilters Entity
            
            # Verify response structure
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "RetrieveEntityResponse"
            $response.EntityMetadata | Should -Not -BeNull
            $response.EntityMetadata.LogicalName | Should -Be "contact"
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RetrieveEntityRequest"
            $proxy.LastRequest.LogicalName | Should -Be "contact"
        }
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

    Context "AddMembersTeam SDK Cmdlet" {
        It "Invoke-DataverseAddMembersTeam adds users to a team" {
            # Create a team entity first
            $connection = getMockConnection -AdditionalEntities @("team")
            $team = New-Object Microsoft.Xrm.Sdk.Entity("team")
            $teamId = $team.Id = $team["teamid"] = [Guid]::NewGuid()
            $team["name"] = "Test Team"
            $team | Set-DataverseRecord -Connection $connection
            
            $userId1 = [Guid]::NewGuid()
            $userId2 = [Guid]::NewGuid()
            
            # Call the cmdlet
            $memberIds = @($userId1, $userId2)
            { Invoke-DataverseAddMembersTeam -Connection $connection -TeamId $teamId -MemberIds $memberIds } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $connection
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "AddMembersTeamRequest"
            $proxy.LastRequest.TeamId | Should -Be $teamId
            $proxy.LastRequest.MemberIds.Count | Should -Be 2
            $proxy.LastRequest.MemberIds[0] | Should -Be $userId1
            $proxy.LastRequest.MemberIds[1] | Should -Be $userId2
        }
    }

    Context "SetState SDK Cmdlet" {
        It "Invoke-DataverseSetState changes record state" {
            # Create a workflow record
            $connection = getMockConnection -AdditionalEntities @("workflow")
            $workflowId = [Guid]::NewGuid()
            
            # Stub the SetStateResponse since we're testing the request pattern
            $proxy = Get-ProxyService -Connection $connection
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.SetStateRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.SetStateResponse
                return $response
            })
            
            # Call the SetState cmdlet using the specialized SDK cmdlet
            $entityRef = New-Object Microsoft.Xrm.Sdk.EntityReference("workflow", $workflowId)
            $state = New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)
            $status = New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)
            
            # Note: There's no Invoke-DataverseSetState - SetState is done via generic request
            $setStateRequest = New-Object Microsoft.Crm.Sdk.Messages.SetStateRequest
            $setStateRequest.EntityMoniker = $entityRef
            $setStateRequest.State = $state
            $setStateRequest.Status = $status
            
            { Invoke-DataverseRequest -Connection $connection -Request $setStateRequest } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "SetStateRequest"
            $proxy.LastRequest.EntityMoniker.LogicalName | Should -Be "workflow"
            $proxy.LastRequest.EntityMoniker.Id | Should -Be $workflowId
            $proxy.LastRequest.State.Value | Should -Be 1
            $proxy.LastRequest.Status.Value | Should -Be 2
        }
    }

    Context "PublishDuplicateRule SDK Cmdlet" {
        It "Invoke-DataversePublishDuplicateRule publishes a rule" {
            $ruleId = [Guid]::NewGuid()
            
            # Stub the response since FakeXrmEasy OSS doesn't support this
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PublishDuplicateRuleRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.PublishDuplicateRuleResponse
                $response.Results["JobId"] = [Guid]::NewGuid()
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataversePublishDuplicateRule -Connection $script:conn -DuplicateRuleId $ruleId
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "PublishDuplicateRuleResponse"
            $response.JobId | Should -Not -BeNullOrEmpty
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "PublishDuplicateRuleRequest"
            $proxy.LastRequest.DuplicateRuleId | Should -Be $ruleId
        }
    }

    Context "BulkDelete SDK Cmdlet" {
        It "Invoke-DataverseBulkDelete accepts query and parameters" {
            # Create a query expression
            $query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("contact")
            $query.Criteria.AddCondition("statecode", [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal, 1)
            
            # Call the cmdlet
            $jobName = "Test Bulk Delete"
            $sendNotification = $false
            $toRecipients = @()
            $ccRecipients = @()
            $recurrencePattern = ""
            $startDateTime = [DateTime]::Now
            
            { Invoke-DataverseBulkDelete -Connection $script:conn -QuerySet @($query) -JobName $jobName -SendEmailNotification $sendNotification -ToRecipients $toRecipients -CCRecipients $ccRecipients -RecurrencePattern $recurrencePattern -StartDateTime $startDateTime } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "BulkDeleteRequest"
            $proxy.LastRequest.JobName | Should -Be $jobName
            $proxy.LastRequest.QuerySet.Count | Should -Be 1
            $proxy.LastRequest.QuerySet[0].EntityName | Should -Be "contact"
        }
    }

    Context "ExecuteWorkflow SDK Cmdlet" {
        It "Invoke-DataverseExecuteWorkflow triggers workflow execution" {
            $workflowId = [Guid]::NewGuid()
            $entityId = [Guid]::NewGuid()
            
            # Stub the response since FakeXrmEasy OSS doesn't support this
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExecuteWorkflowRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.ExecuteWorkflowResponse
                $response.Results["Id"] = [Guid]::NewGuid()
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseExecuteWorkflow -Connection $script:conn -WorkflowId $workflowId -EntityId $entityId
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "ExecuteWorkflowResponse"
            $response.Id | Should -Not -BeNullOrEmpty
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "ExecuteWorkflowRequest"
            $proxy.LastRequest.WorkflowId | Should -Be $workflowId
            $proxy.LastRequest.EntityId | Should -Be $entityId
        }
    }

    Context "Additional SDK Cmdlets" {
        It "Invoke-DataverseRetrieveAttribute retrieves attribute metadata" {
            # Call the cmdlet
            $response = Invoke-DataverseRetrieveAttribute -Connection $script:conn -EntityLogicalName "contact" -LogicalName "firstname"
            
            # Verify response structure
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "RetrieveAttributeResponse"
            $response.AttributeMetadata | Should -Not -BeNull
            $response.AttributeMetadata.LogicalName | Should -Be "firstname"
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RetrieveAttributeRequest"
            $proxy.LastRequest.EntityLogicalName | Should -Be "contact"
            $proxy.LastRequest.LogicalName | Should -Be "firstname"
        }

        It "Invoke-DataverseRetrieveMultiple with QueryExpression" {
            # Create test contacts
            $contact1 = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contact1.Id = $contact1["contactid"] = [Guid]::NewGuid()
            $contact1["firstname"] = "Multi1"
            $contact1 | Set-DataverseRecord -Connection $script:conn

            $contact2 = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contact2.Id = $contact2["contactid"] = [Guid]::NewGuid()
            $contact2["firstname"] = "Multi2"
            $contact2 | Set-DataverseRecord -Connection $script:conn
            
            # Create query
            $query = New-Object Microsoft.Xrm.Sdk.Query.QueryExpression("contact")
            $query.ColumnSet = New-Object Microsoft.Xrm.Sdk.Query.ColumnSet("firstname", "lastname")
            
            # Call the cmdlet
            $response = Invoke-DataverseRetrieveMultiple -Connection $script:conn -Query $query
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "RetrieveMultipleResponse"
            $response.EntityCollection | Should -Not -BeNull
            $response.EntityCollection.Entities.Count | Should -BeGreaterThan 0
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RetrieveMultipleRequest"
        }

        It "Invoke-DataverseCreate creates a new entity" {
            # Create entity
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contact["firstname"] = "CreateTest"
            $contact["lastname"] = "User"
            
            # Call the cmdlet
            $response = Invoke-DataverseCreate -Connection $script:conn -Target $contact
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "CreateResponse"
            $response.Id | Should -Not -BeNullOrEmpty
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "CreateRequest"
            $proxy.LastRequest.Target.LogicalName | Should -Be "contact"
        }

        It "Invoke-DataverseUpdate updates an entity" {
            # Create entity first
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "Before"
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Update entity
            $updateContact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $updateContact.Id = $contactId
            $updateContact["firstname"] = "After"
            
            # Call the cmdlet
            { Invoke-DataverseUpdate -Connection $script:conn -Target $updateContact } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "UpdateRequest"
            $proxy.LastRequest.Target.Id | Should -Be $contactId
        }

        It "Invoke-DataverseDelete deletes an entity" {
            # Create entity first
            $contact = New-Object Microsoft.Xrm.Sdk.Entity("contact")
            $contactId = $contact.Id = $contact["contactid"] = [Guid]::NewGuid()
            $contact["firstname"] = "ToDelete"
            $contact | Set-DataverseRecord -Connection $script:conn
            
            # Delete entity
            $entityRef = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contactId)
            
            # Call the cmdlet
            { Invoke-DataverseDelete -Connection $script:conn -Target $entityRef } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy | Should -Not -BeNull
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "DeleteRequest"
            $proxy.LastRequest.Target.Id | Should -Be $contactId
        }
    }

    Context "ExportSolution SDK Cmdlet" {
        It "Invoke-DataverseExportSolution exports a solution" {
            $solutionName = "TestSolution"
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ExportSolutionRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.ExportSolutionResponse
                # Create a fake solution file (just some bytes)
                $response.Results["ExportSolutionFile"] = [System.Text.Encoding]::UTF8.GetBytes("fake solution content")
                return $response
            })
            
            # Call the cmdlet
            $response = Invoke-DataverseExportSolution -Connection $script:conn -SolutionName $solutionName -Managed $false
            
            # Verify response
            $response | Should -Not -BeNull
            $response.GetType().Name | Should -Be "ExportSolutionResponse"
            $response.ExportSolutionFile | Should -Not -BeNull
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "ExportSolutionRequest"
            $proxy.LastRequest.SolutionName | Should -Be $solutionName
            $proxy.LastRequest.Managed | Should -Be $false
        }
    }

    Context "ImportSolution SDK Cmdlet" {
        It "Invoke-DataverseImportSolution imports a solution" {
            $solutionBytes = [System.Text.Encoding]::UTF8.GetBytes("fake solution content")
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.ImportSolutionRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.ImportSolutionResponse
                return $response
            })
            
            # Call the cmdlet
            { Invoke-DataverseImportSolution -Connection $script:conn -CustomizationFile $solutionBytes } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "ImportSolutionRequest"
            $proxy.LastRequest.CustomizationFile | Should -Be $solutionBytes
        }
    }

    Context "PublishAllXml SDK Cmdlet" {
        It "Invoke-DataversePublishAllXml publishes all customizations" {
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.PublishAllXmlRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.PublishAllXmlResponse
                return $response
            })
            
            # Call the cmdlet
            { Invoke-DataversePublishAllXml -Connection $script:conn } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "PublishAllXmlRequest"
        }
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

    Context "RevokeAccess SDK Cmdlet" {
        It "Invoke-DataverseRevokeAccess revokes access to a record" {
            $contactId = [Guid]::NewGuid()
            $userId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RevokeAccessRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.RevokeAccessResponse
                return $response
            })
            
            # Call the cmdlet
            $target = New-Object Microsoft.Xrm.Sdk.EntityReference("contact", $contactId)
            $revokee = New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", $userId)
            
            { Invoke-DataverseRevokeAccess -Connection $script:conn -Target $target -Revokee $revokee } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RevokeAccessRequest"
            $proxy.LastRequest.Target.Id | Should -Be $contactId
            $proxy.LastRequest.Revokee.Id | Should -Be $userId
        }
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

    Context "AddPrivilegesRole SDK Cmdlet" {
        It "Invoke-DataverseAddPrivilegesRole adds privileges to a role" {
            $roleId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.AddPrivilegesRoleRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.AddPrivilegesRoleResponse
                return $response
            })
            
            # Call the cmdlet
            $privilege = New-Object Microsoft.Crm.Sdk.Messages.RolePrivilege
            $privilege.PrivilegeId = [Guid]::NewGuid()
            $privilege.Depth = [Microsoft.Crm.Sdk.Messages.PrivilegeDepth]::Basic
            
            { Invoke-DataverseAddPrivilegesRole -Connection $script:conn -RoleId $roleId -Privileges @($privilege) } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "AddPrivilegesRoleRequest"
            $proxy.LastRequest.RoleId | Should -Be $roleId
        }
    }

    Context "RemoveMemberList SDK Cmdlet" {
        It "Invoke-DataverseRemoveMemberList removes a member from a list" {
            $listId = [Guid]::NewGuid()
            $entityId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.RemoveMemberListRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.RemoveMemberListResponse
                return $response
            })
            
            # Call the cmdlet
            { Invoke-DataverseRemoveMemberList -Connection $script:conn -ListId $listId -EntityId $entityId } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "RemoveMemberListRequest"
            $proxy.LastRequest.ListId | Should -Be $listId
            $proxy.LastRequest.EntityId | Should -Be $entityId
        }
    }

    Context "CloseIncident SDK Cmdlet" {
        It "Invoke-DataverseCloseIncident closes an incident (case)" {
            $incidentId = [Guid]::NewGuid()
            
            # Stub the response
            $proxy = Get-ProxyService -Connection $script:conn
            $proxy.StubResponse("Microsoft.Crm.Sdk.Messages.CloseIncidentRequest", {
                param($request)
                $response = New-Object Microsoft.Crm.Sdk.Messages.CloseIncidentResponse
                return $response
            })
            
            # Create an incident resolution entity
            $resolution = New-Object Microsoft.Xrm.Sdk.Entity("incidentresolution")
            $resolution["subject"] = "Resolved"
            $resolution["incidentid"] = New-Object Microsoft.Xrm.Sdk.EntityReference("incident", $incidentId)
            
            # Call the cmdlet
            { Invoke-DataverseCloseIncident -Connection $script:conn -IncidentResolution $resolution -Status -1 } | Should -Not -Throw
            
            # Verify the proxy captured the request
            $proxy.LastRequest | Should -Not -BeNull
            $proxy.LastRequest.GetType().Name | Should -Be "CloseIncidentRequest"
            $proxy.LastRequest.Status | Should -Be -1
        }
    }
}
