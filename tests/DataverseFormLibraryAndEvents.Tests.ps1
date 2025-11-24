. "$PSScriptRoot\Common.ps1"

Describe 'Dataverse Form Library and Event Handler Cmdlets' {
    BeforeAll {
        $connection = getMockConnection -Entities @('systemform')
        $global:testFormXmlWithEvents = Get-Content "$PSScriptRoot\test.formxml.with-events" -Raw
        
        # Create a mock form with libraries and events
        $script:testFormId = [System.Guid]::NewGuid()
        $script:testForm = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
        $script:testForm["formid"] = $script:testForm.Id = $script:testFormId
        $script:testForm["name"] = "Test Form with Libraries"
        $script:testForm["objecttypecode"] = "contact"
        $script:testForm["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
        $script:testForm["formxml"] = [string]$global:testFormXmlWithEvents
        $connection.Create($script:testForm)
        
        # Helper function to reset form XML to initial state
        function Reset-TestFormXml {
            $script:testForm["formxml"] = [string]$global:testFormXmlWithEvents
            $connection.Update($script:testForm)
        }
    }

    Context 'Get-DataverseFormLibrary' {
        It "Retrieves all libraries from a form" {
            $result = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            
            $result | Should -Not -BeNullOrEmpty
            @($result).Count | Should -Be 2
            $result[0].Name | Should -Be "new_/scripts/main.js"
            $result[1].Name | Should -Be "new_/scripts/validation.js"
            $result[0].FormId | Should -Be $testFormId
            $result[0].LibraryUniqueId | Should -Not -BeNullOrEmpty
        }

        It "Filters libraries by name" {
            $result = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/main.js"
            
            $result | Should -Not -BeNullOrEmpty
            @($result).Count | Should -Be 1
            $result.Name | Should -Be "new_/scripts/main.js"
        }

        It "Filters libraries by unique ID" {
            $allLibs = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            $uniqueId = $allLibs[0].LibraryUniqueId
            
            $result = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryUniqueId $uniqueId
            
            $result | Should -Not -BeNullOrEmpty
            $result.LibraryUniqueId | Should -Be $uniqueId
        }

        It "Returns nothing when form has no libraries" {
            # Create a form without libraries
            $formWithoutLibs = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $formWithoutLibs["formid"] = $formWithoutLibs.Id = [System.Guid]::NewGuid()
            $formWithoutLibs["objecttypecode"] = "contact"
            $formWithoutLibs["formxml"] = "<form></form>"
            $connection.Create($formWithoutLibs)
            
            $result = Get-DataverseFormLibrary -Connection $connection -FormId $formWithoutLibs.Id -ErrorAction SilentlyContinue
            
            $result | Should -BeNullOrEmpty
        }

        It "Throws error when specific library not found" {
            { Get-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "nonexistent.js" -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Set-DataverseFormLibrary' {
        It "Adds a new library to a form" {
            $result = Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/newlibrary.js" -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be "new_/scripts/newlibrary.js"
            $result.LibraryUniqueId | Should -Not -BeNullOrEmpty
            
            # Verify it was added
            $allLibs = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            @($allLibs).Count | Should -Be 3
        }

        It "Updates an existing library" {
            $customId = [System.Guid]::NewGuid()
            
            $result = Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/main.js" -LibraryUniqueId $customId -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.LibraryUniqueId | Should -Be $customId
        }

        It "Creates formLibraries element if it doesn't exist" {
            # Create a form without libraries
            $newForm = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $newForm["formid"] = $newForm.Id = [System.Guid]::NewGuid()
            $newForm["objecttypecode"] = "contact"
            $newForm["formxml"] = "<form></form>"
            $connection.Create($newForm)
            
            $result = Set-DataverseFormLibrary -Connection $connection -FormId $newForm.Id -LibraryName "new_/scripts/main.js" -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be "new_/scripts/main.js"
            
            # Verify library was added
            $libs = Get-DataverseFormLibrary -Connection $connection -FormId $newForm.Id
            $libs | Should -Not -BeNullOrEmpty
        }

        It "Web resource validation in mock tests" {
            # Note: Web resource validation is disabled in mock tests because
            # FakeXrmEasy doesn't support RetrieveUnpublished and we don't have webresource metadata
            # The cmdlets will catch validation in try/catch and skip for mock scenarios
            # This test just verifies the cmdlet works with any library name
            $result = Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "any/resource.js" -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be "any/resource.js"
        }

        It "Supports WhatIf" {
            $initialCount = @(Get-DataverseFormLibrary -Connection $connection -FormId $testFormId).Count
            
            Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/newlibrary.js" -Confirm:$false -WhatIf
            
            $afterCount = @(Get-DataverseFormLibrary -Connection $connection -FormId $testFormId).Count
            $afterCount | Should -Be $initialCount
        }
    }

    Context 'Remove-DataverseFormLibrary' {
        BeforeEach {
            Reset-TestFormXml
        }

        It "Removes a library by name" {
            $initialCount = @(Get-DataverseFormLibrary -Connection $connection -FormId $testFormId).Count
            
            Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/main.js" -Confirm:$false
            
            $afterCount = @(Get-DataverseFormLibrary -Connection $connection -FormId $testFormId).Count
            $afterCount | Should -Be ($initialCount - 1)
            
            $remaining = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            $remaining.Name | Should -Not -Contain "new_/scripts/main.js"
        }

        It "Removes a library by unique ID" {
            $allLibs = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            $libToRemove = $allLibs[0]
            
            Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryUniqueId $libToRemove.LibraryUniqueId -Confirm:$false
            
            $remaining = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId -ErrorAction SilentlyContinue
            $remaining.LibraryUniqueId | Should -Not -Contain $libToRemove.LibraryUniqueId
        }

        It "Removes formLibraries element when last library is removed" {
            # Remove all libraries one by one
            $allLibs = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            foreach ($lib in $allLibs) {
                Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName $lib.Name -Confirm:$false
            }
            
            # Verify no libraries remain
            $remaining = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Throws error when library not found" {
            { Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "nonexistent.js" -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }

        It "Supports WhatIf" {
            $initialCount = @(Get-DataverseFormLibrary -Connection $connection -FormId $testFormId).Count
            
            Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/main.js" -WhatIf
            
            $afterCount = @(Get-DataverseFormLibrary -Connection $connection -FormId $testFormId).Count
            $afterCount | Should -Be $initialCount
        }
    }

    Context 'Get-DataverseFormEventHandler - Form Events' {
        It "Retrieves all form-level event handlers" {
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId
            
            $result | Should -Not -BeNullOrEmpty
            @($result).Count | Should -BeGreaterThan 0
            $result[0].FormId | Should -Be $testFormId
            $result[0].EventName | Should -Not -BeNullOrEmpty
            $result[0].FunctionName | Should -Not -BeNullOrEmpty
            $result[0].LibraryName | Should -Not -BeNullOrEmpty
        }

        It "Filters handlers by event name" {
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload"
            
            $result | Should -Not -BeNullOrEmpty
            foreach ($handler in $result) {
                $handler.EventName | Should -Be "onload"
            }
        }

        It "Filters handlers by unique ID" {
            $allHandlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId
            $uniqueId = $allHandlers[0].HandlerUniqueId
            
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -HandlerUniqueId $uniqueId
            
            $result | Should -Not -BeNullOrEmpty
            $result.HandlerUniqueId | Should -Be $uniqueId
        }

        It "Returns form-level handlers with null ControlId" {
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload"
            
            $result | Should -Not -BeNullOrEmpty
            $result[0].ControlId | Should -BeNullOrEmpty
            $result[0].TabName | Should -BeNullOrEmpty
            $result[0].SectionName | Should -BeNullOrEmpty
        }
    }

    Context 'Get-DataverseFormEventHandler - Control Events' {
        It "Retrieves control-level event handlers" {
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -ControlId "firstname" -TabName "general" -SectionName "name"
            
            $result | Should -Not -BeNullOrEmpty
            $result.ControlId | Should -Be "firstname"
            $result.TabName | Should -Be "general"
            $result.SectionName | Should -Be "name"
            $result.EventName | Should -Be "onchange"
        }

        It "Throws error when control not found" {
            { Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -ControlId "nonexistent" -TabName "general" -SectionName "name" -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Set-DataverseFormEventHandler - Form Events' {
        It "Adds a new form-level event handler" {
            # First add the library
            Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/newlibrary.js" -Confirm:$false
            
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "NewOnLoadFunction" -LibraryName "new_/scripts/newlibrary.js" -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.EventName | Should -Be "onload"
            $result.FunctionName | Should -Be "NewOnLoadFunction"
            $result.LibraryName | Should -Be "new_/scripts/newlibrary.js"
            $result.HandlerUniqueId | Should -Not -BeNullOrEmpty
            $result.ControlId | Should -BeNullOrEmpty
            
            # Verify it was added
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload"
            $handlers.FunctionName | Should -Contain "NewOnLoadFunction"
        }

        It "Updates an existing form-level event handler" {
            $customId = [System.Guid]::NewGuid()
            
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "FormOnLoad" -LibraryName "new_/scripts/main.js" -HandlerUniqueId $customId -Enabled $false -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.HandlerUniqueId | Should -Be $customId
            $result.Enabled | Should -Be $false
        }

        It "Creates events element if it doesn't exist" {
            # Create a form without events but with library
            $newForm = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $newForm["formid"] = $newForm.Id = [System.Guid]::NewGuid()
            $newForm["objecttypecode"] = "contact"
            $newForm["formxml"] = @"
<form>
    <formLibraries>
        <Library name="new_/scripts/main.js" libraryUniqueId="{A1B2C3D4-E5F6-4789-ABCD-EF0123456789}" />
    </formLibraries>
</form>
"@
            $connection.Create($newForm)
            
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $newForm.Id -EventName "onload" -FunctionName "OnLoad" -LibraryName "new_/scripts/main.js" -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify handler was added
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $newForm.Id
            $handlers | Should -Not -BeNullOrEmpty
        }

        It "Supports custom parameters" {
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "ParameterizedFunction" -LibraryName "new_/scripts/main.js" -Parameters "{'key':'value'}" -PassExecutionContext $false -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.Parameters | Should -Be "{'key':'value'}"
            $result.PassExecutionContext | Should -Be $false
        }

        It "Web resource validation in mock tests" {
            # In mock tests, web resource validation is bypassed
            # This test just verifies the cmdlet works with any library name
            # First add the library to the form
            Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "any/resource.js" -Confirm:$false
            
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "Test" -LibraryName "any/resource.js" -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.LibraryName | Should -Be "any/resource.js"
        }
    }

    Context 'Set-DataverseFormEventHandler - Control Events' {
        It "Adds a new control-level event handler" {
            # First add the library
            Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/newlibrary.js" -Confirm:$false
            
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onchange" -FunctionName "NewOnChangeFunction" -LibraryName "new_/scripts/newlibrary.js" -ControlId "lastname" -TabName "general" -SectionName "name" -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.EventName | Should -Be "onchange"
            $result.FunctionName | Should -Be "NewOnChangeFunction"
            $result.ControlId | Should -Be "lastname"
            $result.TabName | Should -Be "general"
            $result.SectionName | Should -Be "name"
            
            # Verify it was added
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -ControlId "lastname" -TabName "general" -SectionName "name"
            $handlers.FunctionName | Should -Contain "NewOnChangeFunction"
        }

        It "Throws error when control not found" {
            { Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onchange" -FunctionName "Test" -LibraryName "new_/scripts/main.js" -ControlId "nonexistent" -TabName "general" -SectionName "name" -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Remove-DataverseFormEventHandler - Form Events' {
        BeforeEach {
            Reset-TestFormXml
        }

        It "Removes a form-level handler by unique ID" {
            $allHandlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload"
            $handlerToRemove = $allHandlers[0]
            $initialCount = @($allHandlers).Count
            
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -HandlerUniqueId $handlerToRemove.HandlerUniqueId -Confirm:$false
            
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -ErrorAction SilentlyContinue
            @($remaining).Count | Should -Be ($initialCount - 1)
        }

        It "Removes a form-level handler by function name and library" {
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave" -FunctionName "ValidateBeforeSave" -LibraryName "new_/scripts/validation.js" -Confirm:$false
            
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave" -ErrorAction SilentlyContinue
            if ($handlers) {
                $handlers.FunctionName | Should -Not -Contain "ValidateBeforeSave"
            }
        }

        It "Removes event element when last handler is removed" {
            # Remove all handlers from onsave event
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave"
            foreach ($handler in $handlers) {
                Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave" -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
            }
            
            # Verify event has no more handlers (might return empty or nothing)
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave" -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Throws error when handler not found" {
            { Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "NonExistent" -LibraryName "nonexistent.js" -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Remove-DataverseFormEventHandler - Control Events' {
        BeforeEach {
            Reset-TestFormXml
        }

        It "Removes a control-level handler" {
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -ControlId "firstname" -TabName "general" -SectionName "name"
            $handlerToRemove = $handlers[0]
            
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onchange" -HandlerUniqueId $handlerToRemove.HandlerUniqueId -ControlId "firstname" -TabName "general" -SectionName "name" -Confirm:$false
            
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -ControlId "firstname" -TabName "general" -SectionName "name" -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Throws error when control not found" {
            { Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onchange" -FunctionName "Test" -LibraryName "test.js" -ControlId "nonexistent" -TabName "general" -SectionName "name" -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Get-DataverseFormEventHandler - Attribute Events (testform2.formxml)' {
        BeforeAll {
            # Create form with testform2.formxml which has attribute-level and tab-level events
            $testForm2Xml = Get-Content "$PSScriptRoot\testform2.formxml" -Raw
            $script:testForm2Id = [System.Guid]::NewGuid()
            $script:testForm2 = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $script:testForm2["formid"] = $script:testForm2.Id = $script:testForm2Id
            $script:testForm2["name"] = "Test Form 2"
            $script:testForm2["objecttypecode"] = "contact"
            $script:testForm2["type"] = [Microsoft.Xrm.Sdk.OptionSetValue]::new(2)
            $script:testForm2["formxml"] = [string]$testForm2Xml
            $connection.Create($script:testForm2)
        }

        It "Retrieves attribute-level event handlers" {
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department"
            
            $result | Should -Not -BeNullOrEmpty
            $result.EventName | Should -Be "onchange"
            $result.Attribute | Should -Be "department"
            $result.FunctionName | Should -Be "ddd"
            $result.LibraryName | Should -Be "msdyn_/helplink.js"
            $result.ControlId | Should -BeNullOrEmpty
            $result.TabName | Should -BeNullOrEmpty
        }

        It "Filters attribute events by event name" {
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department" -EventName "onchange"
            
            $result | Should -Not -BeNullOrEmpty
            $result.EventName | Should -Be "onchange"
            $result.Attribute | Should -Be "department"
        }

        It "Returns Attribute property in output" {
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department"
            
            $result.Attribute | Should -Not -BeNullOrEmpty
            $result.Attribute | Should -Be "department"
        }

        It "Does not return attribute-level events when querying form-level events" {
            # Form-level events should exclude those with attribute property
            $formEvents = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id
            
            foreach ($event in $formEvents) {
                $event.Attribute | Should -BeNullOrEmpty
            }
        }
    }

    Context 'Get-DataverseFormEventHandler - Tab Events (testform2.formxml)' {
        It "Retrieves tab-level event handlers" {
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General"
            
            $result | Should -Not -BeNullOrEmpty
            $result.EventName | Should -Be "tabstatechange"
            $result.TabName | Should -Be "General"
            $result.FunctionName | Should -Be "ontab"
            $result.LibraryName | Should -Be "msdyn_/helplink.js"
            $result.ControlId | Should -BeNullOrEmpty
            $result.SectionName | Should -BeNullOrEmpty
            $result.Attribute | Should -BeNullOrEmpty
        }

        It "Filters tab events by event name" {
            $result = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange"
            
            $result | Should -Not -BeNullOrEmpty
            $result.EventName | Should -Be "tabstatechange"
            $result.TabName | Should -Be "General"
        }

        It "Throws error when tab not found" {
            { Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "NonExistent" -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Set-DataverseFormEventHandler - Attribute Events' {
        BeforeEach {
            # Reset form to initial state
            $resetXml = Get-Content "$PSScriptRoot\testform2.formxml" -Raw
            $testForm2["formxml"] = [string]$resetXml
            $connection.Update($testForm2)
        }

        It "Adds a new attribute-level event handler" {
            # First add the library
            Set-DataverseFormLibrary -Connection $connection -FormId $testForm2Id -LibraryName "new_/scripts/main.js" -Confirm:$false
            
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "firstname" -EventName "onchange" -FunctionName "OnFirstNameChange" -LibraryName "new_/scripts/main.js" -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.EventName | Should -Be "onchange"
            $result.Attribute | Should -Be "firstname"
            $result.FunctionName | Should -Be "OnFirstNameChange"
            $result.HandlerUniqueId | Should -Not -BeNullOrEmpty
            $result.ControlId | Should -BeNullOrEmpty
            $result.TabName | Should -BeNullOrEmpty
            
            # Verify it was added
            $handler = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "firstname"
            $handler.FunctionName | Should -Be "OnFirstNameChange"
        }

        It "Updates an existing attribute-level event handler" {
            # First add a handler
            Set-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department" -EventName "onchange" -FunctionName "ddd" -LibraryName "msdyn_/helplink.js" -Enabled $false -Confirm:$false
            
            # Verify it was updated
            $handler = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department"
            $handler.Enabled | Should -Be $false
        }

        It "Sets attribute property in event element" {
            # Library already on form (msdyn_/helplink.js from testform2.formxml), add new one for test
            Set-DataverseFormLibrary -Connection $connection -FormId $testForm2Id -LibraryName "new_/scripts/main.js" -Confirm:$false
            
            Set-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "lastname" -EventName "onchange" -FunctionName "OnLastNameChange" -LibraryName "new_/scripts/main.js" -Confirm:$false
            
            # Retrieve the form and check the XML contains attribute property
            $form = $connection.Retrieve("systemform", $testForm2Id, (New-Object Microsoft.Xrm.Sdk.Query.ColumnSet @("formxml")))
            $formXml = $form["formxml"]
            $formXml | Should -Match 'attribute="lastname"'
        }
    }

    Context 'Set-DataverseFormEventHandler - Tab Events' {
        BeforeEach {
            # Reset form to initial state
            $resetXml = Get-Content "$PSScriptRoot\testform2.formxml" -Raw
            $testForm2["formxml"] = [string]$resetXml
            $connection.Update($testForm2)
        }

        It "Adds a new tab-level event handler" {
            # First add the library
            Set-DataverseFormLibrary -Connection $connection -FormId $testForm2Id -LibraryName "new_/scripts/main.js" -Confirm:$false
            
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange" -FunctionName "OnTabChange" -LibraryName "new_/scripts/main.js" -Confirm:$false
            
            $result | Should -Not -BeNullOrEmpty
            $result.EventName | Should -Be "tabstatechange"
            $result.TabName | Should -Be "General"
            $result.FunctionName | Should -Be "OnTabChange"
            $result.HandlerUniqueId | Should -Not -BeNullOrEmpty
            $result.ControlId | Should -BeNullOrEmpty
            $result.SectionName | Should -BeNullOrEmpty
            $result.Attribute | Should -BeNullOrEmpty
            
            # Verify there are now 2 handlers for tabstatechange on General tab
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange"
            @($handlers).Count | Should -BeGreaterOrEqual 2
        }

        It "Updates an existing tab-level event handler" {
            # Update the existing handler
            Set-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange" -FunctionName "ontab" -LibraryName "msdyn_/helplink.js" -Enabled $false -Confirm:$false
            
            # Verify it was updated
            $handler = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange" | Where-Object { $_.FunctionName -eq "ontab" }
            $handler.Enabled | Should -Be $false
        }

        It "Throws error when tab not found" {
            # First add the library so we get to the tab validation
            Set-DataverseFormLibrary -Connection $connection -FormId $testForm2Id -LibraryName "test.js" -Confirm:$false
            
            { Set-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "NonExistent" -EventName "tabstatechange" -FunctionName "Test" -LibraryName "test.js" -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Remove-DataverseFormEventHandler - Attribute Events' {
        BeforeEach {
            # Reset form to initial state
            $resetXml = Get-Content "$PSScriptRoot\testform2.formxml" -Raw
            $testForm2["formxml"] = [string]$resetXml
            $connection.Update($testForm2)
        }

        It "Removes an attribute-level handler by unique ID" {
            $handler = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department"
            
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department" -EventName "onchange" -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
            
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department" -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Removes an attribute-level handler by function name and library" {
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department" -EventName "onchange" -FunctionName "ddd" -LibraryName "msdyn_/helplink.js" -Confirm:$false
            
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "department" -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Throws error when attribute event not found" {
            { Remove-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -AttributeName "nonexistent" -EventName "onchange" -FunctionName "test" -LibraryName "test.js" -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Remove-DataverseFormEventHandler - Tab Events' {
        BeforeEach {
            # Reset form to initial state
            $resetXml = Get-Content "$PSScriptRoot\testform2.formxml" -Raw
            $testForm2["formxml"] = [string]$resetXml
            $connection.Update($testForm2)
        }

        It "Removes a tab-level handler by unique ID" {
            $handler = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange"
            
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange" -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
            
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange" -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Removes a tab-level handler by function name and library" {
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange" -FunctionName "ontab" -LibraryName "msdyn_/helplink.js" -Confirm:$false
            
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "tabstatechange" -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Throws error when tab event not found" {
            { Remove-DataverseFormEventHandler -Connection $connection -FormId $testForm2Id -TabName "General" -EventName "nonexistent" -FunctionName "test" -LibraryName "test.js" -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }
}
