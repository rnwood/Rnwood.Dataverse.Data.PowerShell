. $PSScriptRoot/Common.ps1

Describe "View Management Cmdlets" {
    Context "Set-DataverseView - Basic Creation" {
      It "Creates a personal view with simple filter" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest which Set-DataverseView uses
            # to convert simple column/filter parameters to FetchXML. This test requires E2E testing.
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
   # Create a personal view with simple column definitions
       $viewId = Set-DataverseView -PassThru -Connection $connection `
         -Name "Test Personal View" `
                -TableName contact `
        -ViewType "Personal" `
             -Columns @("firstname", "lastname", "emailaddress1") `
    -FilterValues @{firstname = "John"}
   
     $viewId | Should -Not -BeNullOrEmpty
  $viewId | Should -BeOfType [Guid]
        }

It "Creates a system view with hashtable column definitions" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
       $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
       # Create a system view with column configuration
        $viewId = Set-DataverseView -PassThru -Connection $connection `
    -Name "Test System View" `
    -TableName contact `
      -ViewType "System" `
       -Columns @(
      @{name="firstname"; width=150},
  @{name="lastname"; width=150},
               @{name="emailaddress1"; width=200}
         ) `
      -FilterValues @{lastname = "Smith"}
 
 $viewId | Should -Not -BeNullOrEmpty
      $viewId | Should -BeOfType [Guid]
        }

  It "Creates a view with description" -Skip {
    # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
     $viewId = Set-DataverseView -PassThru -Connection $connection `
       -Name "View with Description" `
        -TableName contact `
        -ViewType "Personal" `
           -Description "This is a test view" `
            -Columns @("firstname", "lastname")
            
 $viewId | Should -Not -BeNullOrEmpty
        }

        It "Creates a view with order by" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
        
     $viewId = Set-DataverseView -PassThru -Connection $connection `
      -Name "View with OrderBy" `
          -TableName contact `
                -ViewType "Personal" `
                -Columns @("firstname", "lastname") `
     -OrderBy @("lastname-", "firstname")
   
   $viewId | Should -Not -BeNullOrEmpty
    
            # Verify the view was created with OrderBy
      $view = Get-DataverseView -Connection $connection -Id $viewId
        $view.OrderBy | Should -Be @("lastname-", "firstname")
        }

     It "Creates a view with complex filter" -Skip {
 # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
         $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
     
            # Create view with OR filter
   $viewId = Set-DataverseView -PassThru -Connection $connection `
            -Name "Complex Filter View" `
            -TableName contact `
        -ViewType "Personal" `
     -Columns @("firstname", "lastname") `
      -FilterValues @{firstname = "John"}, @{lastname = "Smith"}
  
  $viewId | Should -Not -BeNullOrEmpty
     }

     It "Creates a view with nested filter groups" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
  $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
    # Create view with AND/OR combinations
            $viewId = Set-DataverseView -PassThru -Connection $connection `
   -Name "Nested Filter View" `
  -TableName contact `
     -ViewType "Personal" `
             -Columns @("firstname", "lastname", "emailaddress1") `
    -FilterValues @{
   and = @(
        @{firstname = "John"},
    @{or = @(@{lastname = "Smith"}, @{lastname = "Doe"})}
      )
        }
            
     $viewId | Should -Not -BeNullOrEmpty
        }

        It "Creates a view as default view" -Skip {
      # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
    
          $viewId = Set-DataverseView -PassThru -Connection $connection `
     -Name "Default View" `
                -TableName contact `
                -ViewType "System" `
    -IsDefault `
         -Columns @("firstname", "lastname")
     
       $viewId | Should -Not -BeNullOrEmpty
        }

        It "Creates a view with specific QueryType" -Skip {
         # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
  
            # Create an Advanced Search view (QueryType = AdvancedSearch)
         $viewId = Set-DataverseView -PassThru -Connection $connection `
    -Name "Advanced Find View" `
                -TableName contact `
     -ViewType "Personal" `
        -QueryType AdvancedSearch `
    -Columns @("firstname", "lastname")
    
        $viewId | Should -Not -BeNullOrEmpty
        }
    }

    Context "Set-DataverseView - FetchXml Creation" {
        It "Creates a view with FetchXml" {
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
     
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
      -ViewType "Personal" `
        -FetchXml $fetchXml
            
 $viewId | Should -Not -BeNullOrEmpty
        }

        It "Creates a view with FetchXml and custom LayoutXml" {
     $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
  
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
        -ViewType "Personal" `
              -FetchXml $fetchXml `
    -LayoutXml $layoutXml

          $viewId | Should -Not -BeNullOrEmpty
        }
    }

    Context "Set-DataverseView - WhatIf Support" {
        It "Supports WhatIf without creating view" {
     $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
            # This should not create a view - use FetchXML to avoid QueryExpressionToFetchXmlRequest
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
            $result = Set-DataverseView -Connection $connection `
                -Name "WhatIf Test" `
                -TableName contact `
                -ViewType "Personal" `
                -FetchXml $fetchXml `
                -WhatIf
            
            # No view ID should be returned
            $result | Should -BeNullOrEmpty
        }
    }

  Context "Column Management" {
        It "Adds columns to existing view" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
      
            # Create a view first
   $viewId = Set-DataverseView -PassThru -Connection $connection `
   -Name "Test View for Modification" `
                -TableName contact `
     -ViewType "Personal" `
     -Columns @("firstname", "lastname")
 
            # Add email column
            Set-DataverseView -Connection $connection `
        -Id $viewId `
          -ViewType "Personal" `
    -AddColumns @("emailaddress1", "telephone1")
       
         # Success if no error thrown
            $true | Should -Be $true
        }
        
        It "Adds columns to existing view without TableName parameter" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
            # This test specifically validates the fix for the issue where TableName was required
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
      
            # Create a view first with FetchXML (which has the entity name)
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View for AddColumns Without TableName" `
                -TableName contact `
                -ViewType "Personal" `
                -FetchXml $fetchXml
 
            # Add columns WITHOUT specifying TableName - should automatically determine it
            # This is the scenario from the issue: updating a view should not require TableName
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -ViewType "Personal" `
                -AddColumns @("emailaddress1", "telephone1")
       
            # Success if no error thrown - the fix should prevent "value cannot be null" error
            $true | Should -Be $true
        }

        It "Removes columns from existing view" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
   $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
       
            # Create a view first
    $viewId = Set-DataverseView -PassThru -Connection $connection `
         -Name "Test View for Removal" `
      -TableName contact `
  -ViewType "Personal" `
         -Columns @("firstname", "lastname", "emailaddress1")
         
    # Remove email column
            Set-DataverseView -Connection $connection `
                -Id $viewId `
    -ViewType "Personal" `
    -RemoveColumns @("emailaddress1")
       
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Updates column properties" -Skip {
         # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
         
            # Create a view first
    $viewId = Set-DataverseView -PassThru -Connection $connection `
           -Name "Test View for Update" `
   -TableName contact `
    -ViewType "Personal" `
       -Columns @(@{name="firstname"; width=100})
 
            # Update column width
            Set-DataverseView -Connection $connection `
        -Id $viewId `
        -ViewType "Personal" `
        -UpdateColumns @(@{name="firstname"; width=200})
      
            # Success if no error thrown
 $true | Should -Be $true
        }

        It "Adds columns with configuration" -Skip {
       # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
  $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
          
# Create a view first
    $viewId = Set-DataverseView -PassThru -Connection $connection `
           -Name "Test View for Column Config" `
          -TableName contact `
      -ViewType "Personal" `
           -Columns @("firstname")
        
            # Add columns with configuration
  Set-DataverseView -Connection $connection `
  -Id $viewId `
   -ViewType "Personal" `
          -AddColumns @(
@{name="lastname"; width=150},
      @{name="emailaddress1"; width=250}
                )
            
        # Success if no error thrown
  $true | Should -Be $true
        }

        It "Adds columns before a specific column" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
    $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
 
         # Create a view first
        $viewId = Set-DataverseView -PassThru -Connection $connection `
       -Name "Test View for InsertBefore" `
       -TableName contact `
        -ViewType "Personal" `
    -Columns @("firstname", "lastname", "emailaddress1")
            
      # Add column before lastname
       Set-DataverseView -Connection $connection `
 -Id $viewId `
     -ViewType "Personal" `
            -AddColumns @("middlename") `
       -InsertColumnsBefore "lastname"
            
       # Success if no error thrown
    $true | Should -Be $true
        }

        It "Adds columns after a specific column" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
     $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
  
       # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
        -Name "Test View for InsertAfter" `
              -TableName contact `
          -ViewType "Personal" `
          -Columns @("firstname", "lastname", "emailaddress1")
            
          # Add column after firstname
          Set-DataverseView -Connection $connection `
 -Id $viewId `
        -ViewType "Personal" `
  -AddColumns @("middlename") `
     -InsertColumnsAfter "firstname"

            # Success if no error thrown
         $true | Should -Be $true
        }

        It "Throws error when both InsertBefore and InsertAfter are specified" -Skip {
          # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
          $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
    
     # Create a view first
       $viewId = Set-DataverseView -PassThru -Connection $connection `
         -Name "Test View for Parameter Validation" `
        -TableName contact `
      -ViewType "Personal" `
   -Columns @("firstname", "lastname")
        
            # Try to use both InsertBefore and InsertAfter - should throw
            { 
  Set-DataverseView -Connection $connection `
                    -Id $viewId `
            -ViewType "Personal" `
  -AddColumns @("middlename") `
         -InsertColumnsBefore "firstname" `
          -InsertColumnsAfter "lastname"
            } | Should -Throw "*Cannot specify both*"
        }

        It "Throws error when InsertBefore is used without AddColumns" {
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
        
      # Create a view first with FetchXML (doesn't require QueryExpressionToFetchXmlRequest)
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
-Name "Test View for Parameter Validation 2" `
       -TableName contact `
              -ViewType "Personal" `
           -FetchXml $fetchXml
       
      # Try to use InsertBefore without AddColumns - should throw
   { 
     Set-DataverseView -Connection $connection `
        -Id $viewId `
    -ViewType "Personal" `
 -InsertColumnsBefore "firstname"
        } | Should -Throw "*can only be used with the AddColumns parameter*"
        }
    }

    Context "Filter Management" {
   It "Updates filters in view" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
         
          # Create a view first
            $viewId = Set-DataverseView -PassThru -Connection $connection `
     -Name "Test View for Filter Update" `
      -TableName contact `
                -ViewType "Personal" `
     -Columns @("firstname", "lastname") `
        -FilterValues @{firstname = "John"}
       
       # Update filter
       Set-DataverseView -Connection $connection `
                -Id $viewId `
   -ViewType "Personal" `
           -FilterValues @{lastname = "Smith"}
            
   # Success if no error thrown
            $true | Should -Be $true
  }

        It "Replaces FetchXml in view" {
   $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
          
     # Create a view first with FetchXML
    $originalFetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
    -Name "Test View for FetchXml Update" `
       -TableName contact `
          -ViewType "Personal" `
      -FetchXml $originalFetchXml
            
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
       -ViewType "Personal" `
       -FetchXml $newFetchXml
        
      # Success if no error thrown
    $true | Should -Be $true
        }
        
        It "Updates view without TableName parameter (determines from view metadata)" {
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
            # Create a view first with FetchXML
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test View for TableName" `
                -TableName contact `
                -ViewType "Personal" `
                -FetchXml $fetchXml
            
            # Update view without specifying TableName - should automatically determine it
            # This tests the fix for the issue where TableName was required but should be auto-determined
            Set-DataverseView -Connection $connection `
                -Id $viewId `
                -ViewType "Personal" `
                -Name "Updated Name Without TableName"
            
            # Success if no error thrown
            $true | Should -Be $true
        }
    }

    Context "Metadata Updates" {
        It "Updates view name" {
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
       
   # Create a view first with FetchXML
    $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
   $viewId = Set-DataverseView -PassThru -Connection $connection `
     -Name "Original Name" `
         -TableName contact `
    -ViewType "Personal" `
      -FetchXml $fetchXml
            
         # Update name
     Set-DataverseView -Connection $connection `
            -Id $viewId `
    -ViewType "Personal" `
       -Name "Updated Name"
         
     # Success if no error thrown
            $true | Should -Be $true
 }

        It "Updates view description" {
  $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
  
   # Create a view first with FetchXML
$fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
      $viewId = Set-DataverseView -PassThru -Connection $connection `
 -Name "Test View" `
                -TableName contact `
      -ViewType "Personal" `
      -FetchXml $fetchXml
     
            # Update description
       Set-DataverseView -Connection $connection `
  -Id $viewId `
 -ViewType "Personal" `
     -Description "Updated description"
            
# Success if no error thrown
            $true | Should -Be $true
        }

  It "Sets view as default" {
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
      # Create a system view first with FetchXML
   $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
        $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Test System View" `
  -TableName contact `
         -ViewType "System" `
        -FetchXml $fetchXml
    
            # Set as default
        Set-DataverseView -Connection $connection `
             -Id $viewId `
      -ViewType "System" `
                -IsDefault
  
            # Success if no error thrown
   $true | Should -Be $true
   }

        It "Updates existing view by Name instead of creating duplicate" {
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
            # Create a view first with FetchXML
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Active Contacts" `
                -TableName "contact" `
                -ViewType "System" `
                -FetchXml $fetchXml
            
            $viewId | Should -Not -BeNullOrEmpty
            
            # Update the same view using Name and TableName instead of Id
            # This should update the existing view, not create a duplicate
            $fetchXml2 = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
  </entity>
</fetch>
"@
            $viewId2 = Set-DataverseView -PassThru -Connection $connection `
                -Name "Active Contacts" `
                -TableName "contact" `
                -ViewType "System" `
                -FetchXml $fetchXml2
            
            # Should return the same ID, not a new one
            $viewId2 | Should -Be $viewId
            
            # Verify only one view exists with this name
            $views = Get-DataverseView -Connection $connection -Name "Active Contacts" -TableName "contact" -ViewType "System"
            $views.Count | Should -Be 1
            $views.Id | Should -Be $viewId
        }
    }

    Context "Set-DataverseView - WhatIf Support" {
        It "Supports WhatIf without modifying view" {
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
          
# Create a view first with FetchXML
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
          $viewId = Set-DataverseView -PassThru -Connection $connection `
 -Name "WhatIf Test View" `
        -TableName contact `
          -ViewType "Personal" `
          -FetchXml $fetchXml
            
     # This should not modify the view
  Set-DataverseView -Connection $connection `
    -Id $viewId `
                -ViewType "Personal" `
       -Name "Should Not Update" `
          -WhatIf
         
        # Success if no error thrown
      $true | Should -Be $true
        }
    }

    Context "Remove-DataverseView - Basic Removal" {
        It "Removes a personal view" {
       $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
# Create a view first with FetchXML
    $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
              -Name "Test View to Remove" `
                -TableName contact `
        -ViewType "Personal" `
    -FetchXml $fetchXml
            
  # Remove the view
    Remove-DataverseView -Connection $connection -Id $viewId -ViewType "Personal" -Confirm:$false
   
      # Success if no error thrown
         $true | Should -Be $true
        }

        It "Removes a system view" {
     $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
    # Create a system view first with FetchXML
     $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
            $viewId = Set-DataverseView -PassThru -Connection $connection `
      -Name "System View to Remove" `
      -TableName contact `
         -ViewType "System" `
      -FetchXml $fetchXml
            
            # Remove the system view
 Remove-DataverseView -Connection $connection -Id $viewId -ViewType "System" -Confirm:$false
            
            # Success if no error thrown
            $true | Should -Be $true
        }

        It "Removes multiple views via pipeline" -Skip {
            # Skip: Get-DataverseView retrieval by name pattern requires FetchXmlToQueryExpressionRequest
          $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
            # Create multiple views with FetchXML
       $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
    $viewId1 = Set-DataverseView -PassThru -Connection $connection `
      -Name "View 1" `
          -TableName contact `
   -ViewType "Personal" `
    -FetchXml $fetchXml
            
  $viewId2 = Set-DataverseView -PassThru -Connection $connection `
             -Name "View 2" `
   -TableName contact `
     -ViewType "Personal" `
     -FetchXml $fetchXml
            
    # Get the views and remove via pipeline (realistic scenario)
      Get-DataverseView -Connection $connection -Name "View*" |
    Remove-DataverseView -Connection $connection -Confirm:$false
            
    # Success if no error thrown
 $true | Should -Be $true
        }
    }

  Context "Remove-DataverseView - IfExists Support" {
   It "Does not error when removing non-existent view with IfExists" {
    $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
      
   # Try to remove a view that doesn't exist
      $nonExistentId = [Guid]::NewGuid()
  
  # This should not throw an error
            { 
                Remove-DataverseView -Connection $connection -Id $nonExistentId -ViewType "Personal" -IfExists -Confirm:$false
       } | Should -Not -Throw
     }

        It "Errors when removing non-existent view without IfExists" {
   $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
         
            # Try to remove a view that doesn't exist
     $nonExistentId = [Guid]::NewGuid()
   
        # This should throw an error
            { 
           Remove-DataverseView -Connection $connection -Id $nonExistentId -ViewType "Personal" -ErrorAction Stop -Confirm:$false
      } | Should -Throw
        }
    }

    Context "Remove-DataverseView - WhatIf Support" {
        It "Supports WhatIf without removing view" {
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
            # Create a view first with FetchXML
       $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
    $viewId = Set-DataverseView -PassThru -Connection $connection `
          -Name "WhatIf Remove Test" `
          -TableName contact `
    -ViewType "Personal" `
        -FetchXml $fetchXml
        
            # This should not remove the view
   Remove-DataverseView -Connection $connection -Id $viewId -ViewType "Personal" -WhatIf
 
       # Success if no error thrown
          $true | Should -Be $true
        }
    }

    Context "Integration Tests" {
     It "Creates, modifies, and removes a view in workflow" -Skip {
       # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
            # Create a view
     $viewId = Set-DataverseView -PassThru -Connection $connection `
-Name "Workflow Test View" `
     -TableName contact `
 -ViewType "Personal" `
            -Columns @("firstname", "lastname") `
           -FilterValues @{firstname = "John"}
 
   $viewId | Should -Not -BeNullOrEmpty
    
      # Modify it - add column
  Set-DataverseView -Connection $connection `
       -Id $viewId `
  -ViewType "Personal" `
  -AddColumns @("emailaddress1")

      # Modify it - update name
            Set-DataverseView -Connection $connection `
        -Id $viewId `
        -ViewType "Personal" `
       -Name "Updated Workflow View"
        
            # Remove it
         Remove-DataverseView -Connection $connection -Id $viewId -ViewType "Personal" -Confirm:$false
            
         # Success if no error thrown
       $true | Should -Be $true
}

        It "Creates view with FetchXml then updates metadata" {
      $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
            # Create with FetchXML
$fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
  </entity>
</fetch>
"@
       $viewId = Set-DataverseView -PassThru -Connection $connection `
         -Name "Simple FetchXml View" `
                -TableName contact `
             -ViewType "Personal" `
       -FetchXml $fetchXml
          
   # Update with new FetchXml
$newFetchXml = @"
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
      -ViewType "Personal" `
                -FetchXml $newFetchXml
     
            # Remove
 Remove-DataverseView -Connection $connection -Id $viewId -ViewType "Personal" -Confirm:$false
       
            # Success if no error thrown
  $true | Should -Be $true
 }
    }

  Context "Get-DataverseView - Retrieval" {
        It "Gets all views" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest for creating test views
          $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
     # Create a few views first
       $viewId1 = Set-DataverseView -PassThru -Connection $connection `
        -Name "View 1" `
    -TableName contact `
    -ViewType "Personal" `
    -Columns @("firstname")
  
            $viewId2 = Set-DataverseView -PassThru -Connection $connection `
                -Name "View 2" `
     -TableName contact `
    -ViewType "System" `
       -Columns @("lastname")
     
  # Get all views
      $views = Get-DataverseView -Connection $connection
            
            $views | Should -Not -BeNullOrEmpty
            $views.Count | Should -BeGreaterThan 0
      }

        It "Gets view by ID" {
       $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
   # Create a view with FetchXML
            $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
  </entity>
</fetch>
"@
   $viewId = Set-DataverseView -PassThru -Connection $connection `
       -Name "Test View by ID" `
         -TableName contact `
           -ViewType "Personal" `
    -FetchXml $fetchXml
            
    # Get the view by ID
            $view = Get-DataverseView -Connection $connection -Id $viewId
          
      $view | Should -Not -BeNullOrEmpty
        $view.Name | Should -Be "Test View by ID"
        }

   It "Gets view by name" -Skip {
     # Skip: FakeXrmEasy doesn't support name-based filtering in QueryExpression
     $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
        
         # Create a view with a unique name
    $uniqueName = "Unique View Name $(Get-Random)"
   $fetchXml = @"
<fetch>
  <entity name="contact">
 <attribute name="firstname" />
  </entity>
</fetch>
"@
$viewId = Set-DataverseView -PassThru -Connection $connection `
           -Name $uniqueName `
        -TableName contact `
           -ViewType "Personal" `
                -FetchXml $fetchXml
     
            # Get the view by name
       $view = Get-DataverseView -Connection $connection -Name $uniqueName
            
 $view | Should -Not -BeNullOrEmpty
      $view.Name | Should -Be $uniqueName
        }

        It "Gets views by entity/table name" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest for creating test views
$connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
    # Create views for contact entity
            $viewId1 = Set-DataverseView -PassThru -Connection $connection `
       -Name "Contact View 1" `
           -TableName contact `
            -ViewType "Personal" `
         -Columns @("firstname")
 
    $viewId2 = Set-DataverseView -PassThru -Connection $connection `
          -Name "Contact View 2" `
     -TableName contact `
    -ViewType "Personal" `
       -Columns @("lastname")

  # Get all views for contact entity
            $views = Get-DataverseView -Connection $connection -TableName contact
       
  $views | Should -Not -BeNullOrEmpty
       $views.Count | Should -BeGreaterThan 0
   # All returned views should be for contact entity
            $views | ForEach-Object { $_.TableName | Should -Be "contact" }
      }

    It "Gets only system views" -Skip {
    # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest for creating test views
        $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
        
          # Create a system view
            $viewId = Set-DataverseView -PassThru -Connection $connection `
              -Name "System View Test" `
             -TableName contact `
  -ViewType "System" `
             -Columns @("firstname")
            
      # Get only system views
            $views = Get-DataverseView -Connection $connection -ViewType "System"
            
        $views | Should -Not -BeNullOrEmpty
            # All returned views should be system views
            $views | ForEach-Object { $_.ViewType | Should -Be "System" }
        }

        It "Gets only personal views" -Skip {
  # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest for creating test views
       $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
      
    # Create a personal view
            $viewId = Set-DataverseView -PassThru -Connection $connection `
        -Name "Personal View Test" `
       -TableName contact `
          -ViewType "Personal" `
    -Columns @("firstname")

    # Get only personal views
    $views = Get-DataverseView -Connection $connection -ViewType "Personal"
        
       $views | Should -Not -BeNullOrEmpty
 # All returned views should be personal views
            $views | ForEach-Object { $_.ViewType | Should -Be "Personal" }
        }

        It "Gets views by query type" -Skip {
   # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest for creating test views
        $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
     
            # Create a view with specific query type (Advanced Find = 2)
            $viewId = Set-DataverseView -PassThru -Connection $connection `
      -Name "Advanced Find View" `
       -TableName contact `
           -ViewType "Personal" `
-QueryType AdvancedSearch `
        -Columns @("firstname")
    
     # Get views by query type
     $views = Get-DataverseView -Connection $connection -QueryType AdvancedSearch
          
        $views | Should -Not -BeNullOrEmpty
            # All returned views should have query type 2
        $views | ForEach-Object { $_.QueryType | Should -Be "AdvancedSearch" }
        }

        It "Gets views with wildcard name" -Skip {
            # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest for creating test views
       $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
 # Create views with similar names
            $viewId1 = Set-DataverseView -PassThru -Connection $connection `
           -Name "Test View Alpha" `
       -TableName contact `
  -ViewType "Personal" `
              -Columns @("firstname")
            
  $viewId2 = Set-DataverseView -PassThru -Connection $connection `
         -Name "Test View Beta" `
       -TableName contact `
                -ViewType "Personal" `
         -Columns @("lastname")
            
            # Get views using wildcard
            $views = Get-DataverseView -Connection $connection -Name "Test View*"
  
         $views | Should -Not -BeNullOrEmpty
            $views.Count | Should -BeGreaterThan 0
          # All returned views should match the pattern
   $views | ForEach-Object { $_.Name | Should -BeLike "Test View*" }
        }

        It "Combines filters for entity and system view" -Skip {
     # Skip: FakeXrmEasy doesn't support QueryExpressionToFetchXmlRequest for creating test views
            $connection = getMockConnection -Entities @("savedquery", "userquery", "contact")
            
            # Create a system view for contact
   $viewId = Set-DataverseView -PassThru -Connection $connection `
                -Name "Contact System View" `
    -TableName contact `
          -ViewType "System" `
      -Columns @("firstname", "lastname")
          
     # Get system views for contact entity
      $views = Get-DataverseView -Connection $connection -TableName contact -ViewType "System"
          
     $views | Should -Not -BeNullOrEmpty
            # All returned views should be system views for contact
            $views | ForEach-Object { 
   $_.ViewType | Should -Be "System"
    $_.TableName | Should -Be "contact"
   }
    }
    }
}
