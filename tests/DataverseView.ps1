Describe "View Management Cmdlets" {
    Context "Set-DataverseView - Basic Creation" {
        It "Creates a personal view with simple filter" {
            $connection = getMockConnection
            
            # Create a personal view with simple column definitions
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test Personal View" `
                -TableName contact `
                -Columns @("firstname", "lastname", "emailaddress1") `
                -FilterValues @{firstname = "John"}
            
            $viewId | Should -Not -BeNullOrEmpty
            $viewId | Should -BeOfType [Guid]
        }

        It "Creates a system view with hashtable column definitions" {
            $connection = getMockConnection
            
            # Create a system view with column configuration
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test System View" `
                -TableName contact `
                -SystemView `
                -Columns @(
                    @{name="firstname"; width=150},
                    @{name="lastname"; width=150},
                    @{name="emailaddress1"; width=200}
                ) `
                -FilterValues @{lastname = "Smith"}
            
            $viewId | Should -Not -BeNullOrEmpty
            $viewId | Should -BeOfType [Guid]
        }

        It "Creates a view with description" {
            $connection = getMockConnection
            
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "View with Description" `
                -TableName contact `
                -Description "This is a test view" `
                -Columns @("firstname", "lastname")
            
            $viewId | Should -Not -BeNullOrEmpty
        }

        It "Creates a view with complex filter" {
            $connection = getMockConnection
            
            # Create view with OR filter
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Complex Filter View" `
                -TableName contact `
                -Columns @("firstname", "lastname") `
                -FilterValues @{firstname = "John"}, @{lastname = "Smith"}
            
            $viewId | Should -Not -BeNullOrEmpty
        }

        It "Creates a view with nested filter groups" {
            $connection = getMockConnection
            
            # Create view with AND/OR combinations
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Nested Filter View" `
                -TableName contact `
                -Columns @("firstname", "lastname", "emailaddress1") `
                -FilterValues @{
                    and = @(
                        @{firstname = "John"},
                        @{or = @(@{lastname = "Smith"}, @{lastname = "Doe"})}
                    )
                }
            
            $viewId | Should -Not -BeNullOrEmpty
        }

        It "Creates a view as default view" {
            $connection = getMockConnection
            
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Default View" `
                -TableName contact `
                -SystemView `
                -IsDefault `
                -Columns @("firstname", "lastname")
            
            $viewId | Should -Not -BeNullOrEmpty
        }

        It "Creates a view with specific QueryType" {
            $connection = getMockConnection
            
            # Create an Advanced Find view (QueryType = 2)
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Advanced Find View" `
                -TableName contact `
                -QueryType 2 `
                -Columns @("firstname", "lastname")
            
            $viewId | Should -Not -BeNullOrEmpty
        }
    }

    Context "Set-DataverseView - FetchXml Creation" {
        It "Creates a view with FetchXml" {
            $connection = getMockConnection
            
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <filter type="and">
      <condition attribute="firstname" operator="eq" value="John" />
    </filter>
  </entity>
</fetch>
"@
            
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "FetchXml View" `
                -TableName contact `
                -FetchXml $fetchXml
            
            $viewId | Should -Not -BeNullOrEmpty
        }

        It "Creates a view with FetchXml and custom LayoutXml" {
            $connection = getMockConnection
            
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
  </entity>
</fetch>
"@
            
            $layoutXml = @"
<grid name="resultset" object="contact" jump="contactid" select="1" icon="1" preview="1">
  <row name="result" id="contactid">
    <cell name="firstname" width="150" />
    <cell name="lastname" width="150" />
  </row>
</grid>
"@
            
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "FetchXml with Layout" `
                -TableName contact `
                -FetchXml $fetchXml `
                -LayoutXml $layoutXml
            
            $viewId | Should -Not -BeNullOrEmpty
        }
    }

    Context "Set-DataverseView - WhatIf Support" {
        It "Supports WhatIf without creating view" {
            $connection = getMockConnection
            
            # This should not create a view
            $result = Set-DataverseView -Connection $connection `
                -Name "WhatIf Test" `
                -TableName contact `
                -Columns @("firstname") `
                -WhatIf
            
            # No view ID should be returned
            $result | Should -BeNullOrEmpty
        }
    }

    Context "Column Management" {
        It "Adds columns to existing view" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View for Modification" `
                -TableName contact `
                -Columns @("firstname", "lastname")
            
            # Add email column
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -AddColumns @("emailaddress1", "telephone1")
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Removes columns from existing view" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View for Removal" `
                -TableName contact `
                -Columns @("firstname", "lastname", "emailaddress1")
            
            # Remove email column
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -RemoveColumns @("emailaddress1")
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Updates column properties" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View for Update" `
                -TableName contact `
                -Columns @(@{name="firstname"; width=100})
            
            # Update column width
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -UpdateColumns @(@{name="firstname"; width=200})
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Adds columns with configuration" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View for Column Config" `
                -TableName contact `
                -Columns @("firstname")
            
            # Add columns with configuration
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -AddColumns @(
                    @{name="lastname"; width=150},
                    @{name="emailaddress1"; width=250}
                )
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }

    Context "Filter Management" {
        It "Updates filters in view" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View for Filter Update" `
                -TableName contact `
                -Columns @("firstname", "lastname") `
                -FilterValues @{firstname = "John"}
            
            # Update filter
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -FilterValues @{lastname = "Smith"}
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Replaces FetchXml in view" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View for FetchXml Update" `
                -TableName contact `
                -Columns @("firstname")
            
            # Replace with new FetchXml
            $newFetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
    <filter type="and">
      <condition attribute="lastname" operator="eq" value="Updated" />
    </filter>
  </entity>
</fetch>
"@
            
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -FetchXml $newFetchXml
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }

    Context "Metadata Updates" {
        It "Updates view name" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Original Name" `
                -TableName contact `
                -Columns @("firstname")
            
            # Update name
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -Name "Updated Name"
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Updates view description" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View" `
                -TableName contact `
                -Columns @("firstname")
            
            # Update description
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -Description "Updated description"
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Sets view as default" {
            $connection = getMockConnection
            
            # Create a system view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test System View" `
                -TableName contact `
                -SystemView `
                -Columns @("firstname")
            
            # Set as default
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -SystemView `
                -IsDefault
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }

    Context "Set-DataverseView - WhatIf Support" {
        It "Supports WhatIf without modifying view" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "WhatIf Test View" `
                -TableName contact `
                -Columns @("firstname")
            
            # This should not modify the view
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -Name "Should Not Update" `
                -WhatIf
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }

    Context "Remove-DataverseView - Basic Removal" {
        It "Removes a personal view" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View to Remove" `
                -TableName contact `
                -Columns @("firstname")
            
            # Remove the view
            Remove-DataverseView -Connection $connection -Id $viewId -Confirm:$false
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Removes a system view" {
            $connection = getMockConnection
            
            # Create a system view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "System View to Remove" `
                -TableName contact `
                -SystemView `
                -Columns @("firstname")
            
            # Remove the system view
            Remove-DataverseView -Connection $connection -Id $viewId -SystemView -Confirm:$false
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Removes multiple views via pipeline" {
            $connection = getMockConnection
            
            # Create multiple views
            $viewId1 = Set-DataverseView -PassThru -Connection $connection `
                -Name "View 1" `
                -TableName contact `
                -Columns @("firstname")
            
            $viewId2 = Set-DataverseView -PassThru -Connection $connection `
                -Name "View 2" `
                -TableName contact `
                -Columns @("lastname")
            
            # Remove via pipeline
            @(@{Id=$viewId1}, @{Id=$viewId2}) | Remove-DataverseView -Connection $connection -Confirm:$false
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }

    Context "Remove-DataverseView - IfExists Support" {
        It "Does not error when removing non-existent view with IfExists" {
            $connection = getMockConnection
            
            # Try to remove a view that doesn't exist
            $nonExistentId = [Guid]::NewGuid()
            
            # This should not throw an error
            { 
                Remove-DataverseView -Connection $connection -Id $nonExistentId -IfExists -Confirm:$false
            } | Should -Not -Throw
        }

        It "Errors when removing non-existent view without IfExists" {
            $connection = getMockConnection
            
            # Try to remove a view that doesn't exist
            $nonExistentId = [Guid]::NewGuid()
            
            # This should throw an error
            { 
                Remove-DataverseView -Connection $connection -Id $nonExistentId -ErrorAction Stop -Confirm:$false
            } | Should -Throw
        }
    }

    Context "Remove-DataverseView - WhatIf Support" {
        It "Supports WhatIf without removing view" {
            $connection = getMockConnection
            
            # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "WhatIf Remove Test" `
                -TableName contact `
                -Columns @("firstname")
            
            # This should not remove the view
            Remove-DataverseView -Connection $connection -Id $viewId -WhatIf
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }

    Context "Integration Tests" {
        It "Creates, modifies, and removes a view in workflow" {
            $connection = getMockConnection
            
            # Create a view
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Workflow Test View" `
                -TableName contact `
                -Columns @("firstname", "lastname") `
                -FilterValues @{firstname = "John"}
            
            $viewId | Should -Not -BeNullOrEmpty
            
            # Modify it - add column
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -AddColumns @("emailaddress1")
            
            # Modify it - update name
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -Name "Updated Workflow View"
            
            # Remove it
            Remove-DataverseView -Connection $connection -Id $viewId -Confirm:$false
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Creates view with simple syntax then updates with FetchXml" {
            $connection = getMockConnection
            
            # Create with simple syntax
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Simple to FetchXml" `
                -TableName contact `
                -Columns @("firstname") `
                -FilterValues @{firstname = "Test"}
            
            # Update with FetchXml
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
  </entity>
</fetch>
"@
            
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -FetchXml $fetchXml
            
            # Remove
            Remove-DataverseView -Connection $connection -Id $viewId -Confirm:$false
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }
}

    Context "Get-DataverseView - Retrieval" {
        It "Gets all views" {
            $connection = getMockConnection
            
            # Create a few views first
            $viewId1 = Set-DataverseView -PassThru -Connection $connection `
                -Name "View 1" `
                -TableName contact `
                -Columns @("firstname")
            
            $viewId2 = Set-DataverseView -PassThru -Connection $connection `
                -Name "View 2" `
                -TableName contact `
                -SystemView `
                -Columns @("lastname")
            
            # Get all views
            $views = Get-DataverseView -Connection $connection
            
            $views | Should -Not -BeNullOrEmpty
            $views.Count | Should -BeGreaterThan 0
        }

        It "Gets view by ID" {
            $connection = getMockConnection
            
            # Create a view
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View by ID" `
                -TableName contact `
                -Columns @("firstname", "lastname")
            
            # Get the view by ID
            $view = Get-DataverseView -Connection $connection -Id $viewId
            
            $view | Should -Not -BeNullOrEmpty
            $view.name | Should -Be "Test View by ID"
        }

        It "Gets view by name" {
            $connection = getMockConnection
            
            # Create a view with a unique name
            $uniqueName = "Unique View Name $(Get-Random)"
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name $uniqueName `
                -TableName contact `
                -Columns @("firstname")
            
            # Get the view by name
            $view = Get-DataverseView -Connection $connection -Name $uniqueName
            
            $view | Should -Not -BeNullOrEmpty
            $view.name | Should -Be $uniqueName
        }

        It "Gets views by entity/table name" {
            $connection = getMockConnection
            
            # Create views for contact entity
            $viewId1 = Set-DataverseView -PassThru -Connection $connection `
                -Name "Contact View 1" `
                -TableName contact `
                -Columns @("firstname")
            
            $viewId2 = Set-DataverseView -PassThru -Connection $connection `
                -Name "Contact View 2" `
                -TableName contact `
                -Columns @("lastname")
            
            # Get all views for contact entity
            $views = Get-DataverseView -Connection $connection -TableName contact
            
            $views | Should -Not -BeNullOrEmpty
            $views.Count | Should -BeGreaterThan 0
            # All returned views should be for contact entity
            $views | ForEach-Object { $_.returnedtypecode | Should -Be "contact" }
        }

        It "Gets only system views" {
            $connection = getMockConnection
            
            # Create a system view
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "System View Test" `
                -TableName contact `
                -SystemView `
                -Columns @("firstname")
            
            # Get only system views
            $views = Get-DataverseView -Connection $connection -SystemView
            
            $views | Should -Not -BeNullOrEmpty
            # All returned views should be system views
            $views | ForEach-Object { $_.ViewType | Should -Be "System" }
        }

        It "Gets only personal views" {
            $connection = getMockConnection
            
            # Create a personal view
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Personal View Test" `
                -TableName contact `
                -Columns @("firstname")
            
            # Get only personal views
            $views = Get-DataverseView -Connection $connection -PersonalView
            
            $views | Should -Not -BeNullOrEmpty
            # All returned views should be personal views
            $views | ForEach-Object { $_.ViewType | Should -Be "Personal" }
        }

        It "Gets views by query type" {
            $connection = getMockConnection
            
            # Create a view with specific query type (Advanced Find = 2)
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Advanced Find View" `
                -TableName contact `
                -QueryType 2 `
                -Columns @("firstname")
            
            # Get views by query type
            $views = Get-DataverseView -Connection $connection -QueryType 2
            
            $views | Should -Not -BeNullOrEmpty
            # All returned views should have query type 2
            $views | ForEach-Object { $_.querytype | Should -Be 2 }
        }

        It "Gets views with wildcard name" {
            $connection = getMockConnection
            
            # Create views with similar names
            $viewId1 = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View Alpha" `
                -TableName contact `
                -Columns @("firstname")
            
            $viewId2 = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View Beta" `
                -TableName contact `
                -Columns @("lastname")
            
            # Get views using wildcard
            $views = Get-DataverseView -Connection $connection -Name "Test View*"
            
            $views | Should -Not -BeNullOrEmpty
            $views.Count | Should -BeGreaterThan 0
            # All returned views should match the pattern
            $views | ForEach-Object { $_.name | Should -BeLike "Test View*" }
        }

        It "Combines filters for entity and system view" {
            $connection = getMockConnection
            
            # Create a system view for contact
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Contact System View" `
                -TableName contact `
                -SystemView `
                -Columns @("firstname", "lastname")
            
            # Get system views for contact entity
            $views = Get-DataverseView -Connection $connection -TableName contact -SystemView
            
            $views | Should -Not -BeNullOrEmpty
            # All returned views should be system views for contact
            $views | ForEach-Object { 
                $_.ViewType | Should -Be "System"
                $_.returnedtypecode | Should -Be "contact"
            }
        }
    }
