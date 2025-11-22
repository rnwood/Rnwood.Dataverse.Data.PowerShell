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
            $result = Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/newlibrary.js" -SkipPublish
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be "new_/scripts/newlibrary.js"
            $result.LibraryUniqueId | Should -Not -BeNullOrEmpty
            
            # Verify it was added
            $allLibs = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            @($allLibs).Count | Should -Be 3
        }

        It "Updates an existing library" {
            $customId = [System.Guid]::NewGuid()
            
            $result = Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/main.js" -LibraryUniqueId $customId -SkipPublish
            
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
            
            $result = Set-DataverseFormLibrary -Connection $connection -FormId $newForm.Id -LibraryName "new_/scripts/main.js" -SkipPublish
            
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
            $result = Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "any/resource.js" -SkipPublish
            
            $result | Should -Not -BeNullOrEmpty
            $result.Name | Should -Be "any/resource.js"
        }

        It "Supports WhatIf" {
            $initialCount = @(Get-DataverseFormLibrary -Connection $connection -FormId $testFormId).Count
            
            Set-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/newlibrary.js" -SkipPublish -WhatIf
            
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
            
            Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/main.js" -SkipPublish -Confirm:$false
            
            $afterCount = @(Get-DataverseFormLibrary -Connection $connection -FormId $testFormId).Count
            $afterCount | Should -Be ($initialCount - 1)
            
            $remaining = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            $remaining.Name | Should -Not -Contain "new_/scripts/main.js"
        }

        It "Removes a library by unique ID" {
            $allLibs = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            $libToRemove = $allLibs[0]
            
            Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryUniqueId $libToRemove.LibraryUniqueId -SkipPublish -Confirm:$false
            
            $remaining = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId -ErrorAction SilentlyContinue
            $remaining.LibraryUniqueId | Should -Not -Contain $libToRemove.LibraryUniqueId
        }

        It "Removes formLibraries element when last library is removed" {
            # Remove all libraries one by one
            $allLibs = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId
            foreach ($lib in $allLibs) {
                Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName $lib.Name -SkipPublish -Confirm:$false
            }
            
            # Verify no libraries remain
            $remaining = Get-DataverseFormLibrary -Connection $connection -FormId $testFormId -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Throws error when library not found" {
            { Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "nonexistent.js" -SkipPublish -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }

        It "Supports WhatIf" {
            $initialCount = @(Get-DataverseFormLibrary -Connection $connection -FormId $testFormId).Count
            
            Remove-DataverseFormLibrary -Connection $connection -FormId $testFormId -LibraryName "new_/scripts/main.js" -SkipPublish -WhatIf
            
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

        It "Throws error when TabName and SectionName missing for control events" {
            { Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -ControlId "firstname" -ErrorAction Stop } | Should -Throw "*required*"
        }

        It "Throws error when control not found" {
            { Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -ControlId "nonexistent" -TabName "general" -SectionName "name" -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Set-DataverseFormEventHandler - Form Events' {
        It "Adds a new form-level event handler" {
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "NewOnLoadFunction" -LibraryName "new_/scripts/newlibrary.js" -SkipPublish
            
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
            
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "FormOnLoad" -LibraryName "new_/scripts/main.js" -HandlerUniqueId $customId -Enabled $false -SkipPublish
            
            $result | Should -Not -BeNullOrEmpty
            $result.HandlerUniqueId | Should -Be $customId
            $result.Enabled | Should -Be $false
        }

        It "Creates events element if it doesn't exist" {
            # Create a form without events
            $newForm = New-Object Microsoft.Xrm.Sdk.Entity "systemform"
            $newForm["formid"] = $newForm.Id = [System.Guid]::NewGuid()
            $newForm["objecttypecode"] = "contact"
            $newForm["formxml"] = "<form></form>"
            $connection.Create($newForm)
            
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $newForm.Id -EventName "onload" -FunctionName "OnLoad" -LibraryName "new_/scripts/main.js" -SkipPublish
            
            $result | Should -Not -BeNullOrEmpty
            
            # Verify handler was added
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $newForm.Id
            $handlers | Should -Not -BeNullOrEmpty
        }

        It "Supports custom parameters" {
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "ParameterizedFunction" -LibraryName "new_/scripts/main.js" -Parameters "{'key':'value'}" -PassExecutionContext $false -SkipPublish
            
            $result | Should -Not -BeNullOrEmpty
            $result.Parameters | Should -Be "{'key':'value'}"
            $result.PassExecutionContext | Should -Be $false
        }

        It "Web resource validation in mock tests" {
            # In mock tests, web resource validation is bypassed
            # This test just verifies the cmdlet works with any library name
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "Test" -LibraryName "any/resource.js" -SkipPublish
            
            $result | Should -Not -BeNullOrEmpty
            $result.LibraryName | Should -Be "any/resource.js"
        }
    }

    Context 'Set-DataverseFormEventHandler - Control Events' {
        It "Adds a new control-level event handler" {
            $result = Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onchange" -FunctionName "NewOnChangeFunction" -LibraryName "new_/scripts/newlibrary.js" -ControlId "lastname" -TabName "general" -SectionName "name" -SkipPublish
            
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
            { Set-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onchange" -FunctionName "Test" -LibraryName "new_/scripts/main.js" -ControlId "nonexistent" -TabName "general" -SectionName "name" -SkipPublish -ErrorAction Stop } | Should -Throw "*not found*"
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
            
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -HandlerUniqueId $handlerToRemove.HandlerUniqueId -SkipPublish -Confirm:$false
            
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -ErrorAction SilentlyContinue
            @($remaining).Count | Should -Be ($initialCount - 1)
        }

        It "Removes a form-level handler by function name and library" {
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave" -FunctionName "ValidateBeforeSave" -LibraryName "new_/scripts/validation.js" -SkipPublish -Confirm:$false
            
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave" -ErrorAction SilentlyContinue
            if ($handlers) {
                $handlers.FunctionName | Should -Not -Contain "ValidateBeforeSave"
            }
        }

        It "Removes event element when last handler is removed" {
            # Remove all handlers from onsave event
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave"
            foreach ($handler in $handlers) {
                Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave" -HandlerUniqueId $handler.HandlerUniqueId -SkipPublish -Confirm:$false
            }
            
            # Verify event has no more handlers (might return empty or nothing)
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onsave" -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Throws error when handler not found" {
            { Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onload" -FunctionName "NonExistent" -LibraryName "nonexistent.js" -SkipPublish -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }

    Context 'Remove-DataverseFormEventHandler - Control Events' {
        BeforeEach {
            Reset-TestFormXml
        }

        It "Removes a control-level handler" {
            $handlers = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -ControlId "firstname" -TabName "general" -SectionName "name"
            $handlerToRemove = $handlers[0]
            
            Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onchange" -HandlerUniqueId $handlerToRemove.HandlerUniqueId -ControlId "firstname" -TabName "general" -SectionName "name" -SkipPublish -Confirm:$false
            
            $remaining = Get-DataverseFormEventHandler -Connection $connection -FormId $testFormId -ControlId "firstname" -TabName "general" -SectionName "name" -ErrorAction SilentlyContinue
            $remaining | Should -BeNullOrEmpty
        }

        It "Throws error when control not found" {
            { Remove-DataverseFormEventHandler -Connection $connection -FormId $testFormId -EventName "onchange" -FunctionName "Test" -LibraryName "test.js" -ControlId "nonexistent" -TabName "general" -SectionName "name" -SkipPublish -Confirm:$false -ErrorAction Stop } | Should -Throw "*not found*"
        }
    }
}
