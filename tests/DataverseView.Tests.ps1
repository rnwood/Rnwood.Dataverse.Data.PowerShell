. $PSScriptRoot/Common.ps1

Describe "View Management Cmdlets" {
    Context "Set-DataverseView - Basic Creation" {
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
            
            # Verify the view was updated (retrieving by ID works in FakeXrmEasy)
            $updatedView = Get-DataverseView -Connection $connection -Id $viewId
            $updatedView | Should -Not -BeNullOrEmpty
            $updatedView.Id | Should -Be $viewId
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
    }
}
