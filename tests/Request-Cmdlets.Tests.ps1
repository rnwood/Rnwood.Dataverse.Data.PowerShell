Describe 'Request Cmdlets' {

    . $PSScriptRoot/Common.ps1

    Describe 'Set-DataverseRecordOwner' {
        It "Assigns a record to a different user" -Skip:$true {
            # Skip: AssignRequest requires entity metadata validation that's not available in FakeXrmEasy mock
            $connection = getMockConnection
            
            # Create a test contact record
            $contactId = [Guid]::NewGuid()
            $newUserId = [Guid]::NewGuid()
            
            $contact = @{
                "contactid" = $contactId
                "firstname" = "Test"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $contact
            
            # Use contact as assignee since systemuser metadata is not loaded in mock
            $response = Set-DataverseRecordOwner -Connection $connection -Target $contactId -TableName "contact" -Assignee $newUserId -AssigneeTableName "contact" -Confirm:$false
            $response | Should -Not -BeNullOrEmpty
            $response.GetType().Name | Should -Be "AssignResponse"
        }

        It "Accepts PSObject with Id and TableName" -Skip:$true {
            # Skip: AssignRequest requires entity metadata validation that's not available in FakeXrmEasy mock
            $connection = getMockConnection
            
            $contactId = [Guid]::NewGuid()
            $newUserId = [Guid]::NewGuid()
            
            $contact = @{
                "contactid" = $contactId
                "firstname" = "Test"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $contact
            
            $record = Get-DataverseRecord -Connection $connection -TableName contact -Id $contactId
            # Use contact as assignee since systemuser metadata is not loaded
            $response = Set-DataverseRecordOwner -Connection $connection -Target $record -Assignee $newUserId -AssigneeTableName "contact" -Confirm:$false
            
            $response | Should -Not -BeNullOrEmpty
        }
    }

    Describe 'Set-DataverseRecordState' {
        It "Changes the state and status of a record" {
            $connection = getMockConnection
            
            $contactId = [Guid]::NewGuid()
            $contact = @{
                "contactid" = $contactId
                "firstname" = "Test"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $contact
            
            # Set state to inactive (skip confirmation)
            $response = Set-DataverseRecordState -Connection $connection -Target $contactId -TableName "contact" -State 1 -Status 2 -Confirm:$false
            
            $response | Should -Not -BeNullOrEmpty
            $response.GetType().Name | Should -Be "SetStateResponse"
        }

        It "Accepts PSObject with Id and TableName" {
            $connection = getMockConnection
            
            $contactId = [Guid]::NewGuid()
            $contact = @{
                "contactid" = $contactId
                "firstname" = "Test"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $contact
            
            $record = Get-DataverseRecord -Connection $connection -TableName contact -Id $contactId
            # Pass the whole record object as Target parameter
            $response = Set-DataverseRecordState -Connection $connection -Target $record -State 0 -Status 1 -Confirm:$false
            
            $response | Should -Not -BeNullOrEmpty
        }
    }

    Describe 'Invoke-DataverseWorkflow' {
        It "Executes a workflow against a record" -Skip:$true {
            # Skip: ExecuteWorkflowRequest not supported by FakeXrmEasy
            $connection = getMockConnection
            
            $entityId = [Guid]::NewGuid()
            $workflowId = [Guid]::NewGuid()
            
            $response = Invoke-DataverseWorkflow -Connection $connection -EntityId $entityId -WorkflowId $workflowId
            $response | Should -Not -BeNullOrEmpty
        }

        It "Accepts input arguments" -Skip:$true {
            # Skip: ExecuteWorkflowRequest not supported by FakeXrmEasy
            $connection = getMockConnection
            
            $entityId = [Guid]::NewGuid()
            $workflowId = [Guid]::NewGuid()
            $args = @{ "Param1" = "Value1"; "Param2" = 123 }
            
            $response = Invoke-DataverseWorkflow -Connection $connection -EntityId $entityId -WorkflowId $workflowId -InputArguments $args
            $response | Should -Not -BeNullOrEmpty
        }
    }

    Describe 'Add-DataverseTeamMembers' {
        It "Adds members to a team" -Skip:$true {
            # Skip: AddMembersTeamRequest requires team entity metadata not loaded in mock
            $connection = getMockConnection
            
            $teamId = [Guid]::NewGuid()
            $memberIds = @([Guid]::NewGuid(), [Guid]::NewGuid())
            
            $response = Add-DataverseTeamMembers -Connection $connection -TeamId $teamId -MemberIds $memberIds -Confirm:$false
            $response | Should -Not -BeNullOrEmpty
            $response.GetType().Name | Should -Be "AddMembersTeamResponse"
        }
    }

    Describe 'Remove-DataverseTeamMembers' {
        It "Removes members from a team" -Skip:$true {
            # Skip: RemoveMembersTeamRequest requires team entity metadata not loaded in mock
            $connection = getMockConnection
            
            $teamId = [Guid]::NewGuid()
            $memberIds = @([Guid]::NewGuid(), [Guid]::NewGuid())
            
            $response = Remove-DataverseTeamMembers -Connection $connection -TeamId $teamId -MemberIds $memberIds -Confirm:$false
            $response | Should -Not -BeNullOrEmpty
            $response.GetType().Name | Should -Be "RemoveMembersTeamResponse"
        }
    }

    Describe 'Grant-DataverseAccess' {
        It "Grants access to a record" {
            $connection = getMockConnection
            
            $contactId = [Guid]::NewGuid()
            $userId = [Guid]::NewGuid()
            
            $contact = @{
                "contactid" = $contactId
                "firstname" = "Test"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $contact
            
            $response = Grant-DataverseAccess -Connection $connection -Target $contactId -TableName "contact" -Principal $userId -PrincipalTableName "contact" -AccessRights ReadAccess -Confirm:$false
            $response | Should -Not -BeNullOrEmpty
        }
    }

    Describe 'Revoke-DataverseAccess' {
        It "Revokes access from a record" {
            $connection = getMockConnection
            
            $contactId = [Guid]::NewGuid()
            $userId = [Guid]::NewGuid()
            
            $contact = @{
                "contactid" = $contactId
                "firstname" = "Test"
            }
            Set-DataverseRecord -Connection $connection -TableName contact -InputObject $contact
            
            $response = Revoke-DataverseAccess -Connection $connection -Target $contactId -TableName "contact" -Revokee $userId -RevokeeTableName "contact" -Confirm:$false
            $response | Should -Not -BeNullOrEmpty
        }
    }

    Describe 'Publish-DataverseCustomization' {
        It "Publishes all customizations" {
            $connection = getMockConnection
            
            $response = Publish-DataverseCustomization -Connection $connection
            $response | Should -Not -BeNullOrEmpty
            $response.GetType().Name | Should -Be "PublishXmlResponse"
        }

        It "Publishes with custom XML" {
            $connection = getMockConnection
            
            $xml = "<importexportxml><entities><entity>account</entity></entities></importexportxml>"
            
            $response = Publish-DataverseCustomization -Connection $connection -ParameterXml $xml
            $response | Should -Not -BeNullOrEmpty
        }
    }
}
