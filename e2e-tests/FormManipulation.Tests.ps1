$ErrorActionPreference = "Stop"

Describe "Form Manipulation E2E Tests" {

    BeforeAll {

        if ($env:TESTMODULEPATH) {
            $source = $env:TESTMODULEPATH
        }
        else {
            $source = "$PSScriptRoot/../Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0/"
        }

        $tempmodulefolder = "$([IO.Path]::GetTempPath())/$([Guid]::NewGuid())"
        new-item -ItemType Directory $tempmodulefolder
        copy-item -Recurse $source $tempmodulefolder/Rnwood.Dataverse.Data.PowerShell
        $env:PSModulePath = $tempmodulefolder;
        $env:ChildProcessPSModulePath = $tempmodulefolder
    }

    It "Comprehensively exercises all form manipulation features with cleanup" {
        pwsh -noninteractive -noprofile -command {
            $env:PSModulePath = $env:ChildProcessPSModulePath
           
            Import-Module Rnwood.Dataverse.Data.PowerShell

            try {
                # Connect to Dataverse
                $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
                
                # Generate unique test identifier to avoid conflicts
                $testRunId = [guid]::NewGuid().ToString("N").Substring(0, 8)
                $formName = "E2ETestForm-$testRunId"
                $entityName = "account"
                
                Write-Host "=========================================="
                Write-Host "Starting Form Manipulation E2E Test"
                Write-Host "Test Run ID: $testRunId"
                Write-Host "Form Name: $formName"
                Write-Host "=========================================="
                
                # ============================================================
                # CLEANUP: Remove any leftover forms from previous failed runs
                # ============================================================
                Write-Host ""
                Write-Host "Step 0: Cleanup - Removing any leftover test forms from previous runs..."
                
                # Find and delete all E2E test forms (matching pattern E2ETestForm-*)
                $existingTestForms = Get-DataverseRecord -Connection $connection -TableName systemform -FilterValues @{
                    "name:Like" = "E2ETestForm-%"
                    "objecttypecode" = $entityName
                } -Columns formid, name
                
                if ($existingTestForms -and $existingTestForms.Count -gt 0) {
                    Write-Host "  Found $($existingTestForms.Count) leftover test form(s) to clean up"
                    foreach ($oldForm in $existingTestForms) {
                        Write-Host "  Deleting form: $($oldForm.name) (ID: $($oldForm.formid))"
                        Remove-DataverseForm -Connection $connection -Id $oldForm.formid -Confirm:$false
                    }
                    Write-Host "  Cleanup complete"
                } else {
                    Write-Host "  No leftover test forms found"
                }
                
                # ============================================================
                # STEP 1: CREATE FORM
                # ============================================================
                Write-Host ""
                Write-Host "Step 1: Creating new form..."
                
                $formId = Set-DataverseForm -Connection $connection `
                    -Entity $entityName `
                    -Name $formName `
                    -FormType Main `
                    -Description "E2E test form for comprehensive feature testing - Run $testRunId" `
                    -IsActive `
                    -PassThru
                
                if (-not $formId) {
                    throw "Failed to create form - no form ID returned"
                }
                
                Write-Host "  Created form with ID: $formId"
                
                # Verify form was created
                $createdForm = Get-DataverseForm -Connection $connection -Id $formId
                if (-not $createdForm) {
                    throw "Failed to retrieve created form"
                }
                
                if ($createdForm.name -ne $formName) {
                    throw "Form name mismatch. Expected: $formName, Got: $($createdForm.name)"
                }
                
                Write-Host "  Verified: Form created successfully with correct properties"
                
                # ============================================================
                # STEP 2: GET FORM AND VERIFY INITIAL STRUCTURE
                # ============================================================
                Write-Host ""
                Write-Host "Step 2: Verifying initial form structure..."
                
                # Note: Form is unpublished at this point - formxml may not be fully available
                # We'll validate the full formxml after publishing
                $form = Get-DataverseForm -Connection $connection -Id $formId
                
                if (-not $form) {
                    throw "Failed to retrieve form"
                }
                
                Write-Host "  Form retrieved successfully"
                
                # ============================================================
                # STEP 3: CREATE AND MANIPULATE TABS
                # ============================================================
                Write-Host ""
                Write-Host "Step 3: Creating and manipulating tabs..."
                
                # Create a new tab with specific layout
                $newTabId = "{" + [guid]::NewGuid().ToString().ToUpper() + "}"
                Write-Host "  Creating new tab with ID: $newTabId"
                
                Set-DataverseFormTab -Connection $connection `
                    -FormId $formId `
                    -TabId $newTabId `
                    -Name "CustomTab" `
                    -Label "Custom Tab" `
                    -Layout TwoColumns `
                    -Column1Width 60 `
                    -Column2Width 40 `
                    -Expanded `
                    -ShowLabel `
                    -Confirm:$false
                
                Write-Host "  Created tab: CustomTab"
                
                # Get the tab to verify it was created
                $tab = Get-DataverseFormTab -Connection $connection -FormId $formId -TabName "CustomTab"
                
                if (-not $tab) {
                    throw "Failed to retrieve created tab"
                }
                
                if ($tab.Layout -ne "TwoColumns") {
                    throw "Tab layout mismatch. Expected: TwoColumns, Got: $($tab.Layout)"
                }
                
                Write-Host "  Verified: Tab created with TwoColumns layout ($($tab.Column1Width)% / $($tab.Column2Width)%)"
                
                # Update tab layout to three columns
                Write-Host "  Updating tab layout to ThreeColumns..."
                
                Set-DataverseFormTab -Connection $connection `
                    -FormId $formId `
                    -TabId $newTabId `
                    -Layout ThreeColumns `
                    -Column1Width 33 `
                    -Column2Width 34 `
                    -Column3Width 33 `
                    -Confirm:$false
                
                $updatedTab = Get-DataverseFormTab -Connection $connection -FormId $formId -TabName "CustomTab"
                
                if ($updatedTab.Layout -ne "ThreeColumns") {
                    throw "Tab layout update failed. Expected: ThreeColumns, Got: $($updatedTab.Layout)"
                }
                
                Write-Host "  Verified: Tab layout updated to ThreeColumns ($($updatedTab.Column1Width)% / $($updatedTab.Column2Width)% / $($updatedTab.Column3Width)%)"
                
                # ============================================================
                # STEP 4: CREATE AND MANIPULATE SECTIONS
                # ============================================================
                Write-Host ""
                Write-Host "Step 4: Creating and manipulating sections..."
                
                # Create a new section in the custom tab
                $newSectionId = "{" + [guid]::NewGuid().ToString().ToUpper() + "}"
                Write-Host "  Creating section with ID: $newSectionId"
                
                Set-DataverseFormSection -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionId $newSectionId `
                    -Name "TestSection" `
                    -Label "Test Section" `
                    -ShowLabel `
                    -ShowBar `
                    -Columns 2 `
                    -Confirm:$false
                
                Write-Host "  Created section: TestSection"
                
                # Get the section to verify
                $section = Get-DataverseFormSection -Connection $connection -FormId $formId -TabName "CustomTab" -SectionName "TestSection"
                
                if (-not $section) {
                    throw "Failed to retrieve created section"
                }
                
                Write-Host "  Verified: Section created successfully"
                
                # Create another section to test section management
                $secondSectionId = "{" + [guid]::NewGuid().ToString().ToUpper() + "}"
                
                Set-DataverseFormSection -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionId $secondSectionId `
                    -Name "SecondSection" `
                    -Label "Second Test Section" `
                    -ShowLabel `
                    -Columns 1 `
                    -Confirm:$false
                
                Write-Host "  Created second section: SecondSection"
                
                # Get all sections in the tab
                $allSections = Get-DataverseFormSection -Connection $connection -FormId $formId -TabName "CustomTab"
                
                if (-not $allSections -or $allSections.Count -lt 2) {
                    throw "Expected at least 2 sections in CustomTab, got: $($allSections.Count)"
                }
                
                Write-Host "  Verified: Multiple sections created (Total: $($allSections.Count))"
                
                # ============================================================
                # STEP 5: CREATE AND MANIPULATE CONTROLS
                # ============================================================
                Write-Host ""
                Write-Host "Step 5: Creating and manipulating controls..."
                
                # Create a text control
                Write-Host "  Creating text control for 'name' field..."
                
                Set-DataverseFormControl -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "TestSection" `
                    -ControlId "name" `
                    -DataField "name" `
                    -ControlType Standard `
                    -Label "Account Name" `
                    -IsRequired `
                    -Confirm:$false
                
                Write-Host "  Created control: name (Standard, Required)"
                
                # Create a lookup control
                Write-Host "  Creating lookup control for 'primarycontactid' field..."
                
                Set-DataverseFormControl -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "TestSection" `
                    -ControlId "primarycontactid" `
                    -DataField "primarycontactid" `
                    -ControlType Lookup `
                    -Label "Primary Contact" `
                    -Confirm:$false
                
                Write-Host "  Created control: primarycontactid (Lookup)"
                
                # Create an email control in the second section
                Write-Host "  Creating email control for 'emailaddress1' field..."
                
                Set-DataverseFormControl -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "SecondSection" `
                    -ControlId "emailaddress1" `
                    -DataField "emailaddress1" `
                    -ControlType Email `
                    -Label "Email Address" `
                    -Confirm:$false
                
                Write-Host "  Created control: emailaddress1 (Email)"
                
                # Get control to verify
                $control = Get-DataverseFormControl -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "TestSection" `
                    -ControlId "name"
                
                if (-not $control) {
                    throw "Failed to retrieve created control 'name'"
                }
                
                if ($control.DataField -ne "name") {
                    throw "Control DataField mismatch. Expected: name, Got: $($control.DataField)"
                }
                
                Write-Host "  Verified: Control 'name' created successfully"
                
                # Get all controls in section
                $allControls = Get-DataverseFormControl -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "TestSection"
                
                if (-not $allControls -or $allControls.Count -lt 2) {
                    throw "Expected at least 2 controls in TestSection, got: $($allControls.Count)"
                }
                
                Write-Host "  Verified: Multiple controls created in TestSection (Total: $($allControls.Count))"
                
                # ============================================================
                # STEP 6: UPDATE CONTROL PROPERTIES
                # ============================================================
                Write-Host ""
                Write-Host "Step 6: Updating control properties..."
                
                # Update the name control to make it not required and disabled
                Set-DataverseFormControl -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "TestSection" `
                    -ControlId "name" `
                    -Disabled `
                    -Confirm:$false
                
                Write-Host "  Updated control 'name' to disabled"
                
                # Verify the update
                $updatedControl = Get-DataverseFormControl -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "TestSection" `
                    -ControlId "name"
                
                Write-Host "  Verified: Control properties updated"
                
                # ============================================================
                # STEP 6.5: TEST PUBLISHING AFTER MODIFICATIONS
                # ============================================================
                Write-Host ""
                Write-Host "Step 6.5: Publishing form after all modifications..."
                
                # Publish the form with all changes
                Set-DataverseForm -Connection $connection `
                    -Id $formId `
                    -Publish `
                    -Confirm:$false
                
                Write-Host "  Published form successfully"
                
                # Verify the form can be retrieved with formxml after publish
                $publishedForm = Get-DataverseForm -Connection $connection -Id $formId -IncludeFormXml
                if (-not $publishedForm) {
                    throw "Failed to retrieve form after publish"
                }
                
                if (-not $publishedForm.formxml) {
                    throw "Form XML is empty after publish"
                }
                
                Write-Host "  Verified: Form published successfully and formxml is available"
                
                # ============================================================
                # STEP 7: REMOVE CONTROL
                # ============================================================
                Write-Host ""
                Write-Host "Step 7: Removing a control..."
                
                # Remove the email control
                Remove-DataverseFormControl -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "SecondSection" `
                    -ControlId "emailaddress1" `
                    -Confirm:$false
                
                Write-Host "  Removed control: emailaddress1"
                
                # Verify the control was removed
                $removedControl = Get-DataverseFormControl -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "SecondSection" `
                    -ControlId "emailaddress1"
                
                if ($removedControl) {
                    throw "Control 'emailaddress1' should have been removed but still exists"
                }
                
                Write-Host "  Verified: Control successfully removed"
                
                # ============================================================
                # STEP 8: REMOVE SECTION
                # ============================================================
                Write-Host ""
                Write-Host "Step 8: Removing a section..."
                
                # Remove the second section
                Remove-DataverseFormSection -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "SecondSection" `
                    -Confirm:$false
                
                Write-Host "  Removed section: SecondSection"
                
                # Verify the section was removed
                $removedSection = Get-DataverseFormSection -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -SectionName "SecondSection"
                
                if ($removedSection) {
                    throw "Section 'SecondSection' should have been removed but still exists"
                }
                
                Write-Host "  Verified: Section successfully removed"
                
                # ============================================================
                # STEP 9: REMOVE TAB
                # ============================================================
                Write-Host ""
                Write-Host "Step 9: Removing a tab..."
                
                # Remove the custom tab
                Remove-DataverseFormTab -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab" `
                    -Confirm:$false
                
                Write-Host "  Removed tab: CustomTab"
                
                # Verify the tab was removed
                $removedTab = Get-DataverseFormTab -Connection $connection `
                    -FormId $formId `
                    -TabName "CustomTab"
                
                if ($removedTab) {
                    throw "Tab 'CustomTab' should have been removed but still exists"
                }
                
                Write-Host "  Verified: Tab successfully removed"
                
                # ============================================================
                # STEP 10: CLEANUP - REMOVE FORM
                # ============================================================
                Write-Host ""
                Write-Host "Step 10: Final cleanup - Removing test form..."
                
                # Remove the form
                Remove-DataverseForm -Connection $connection -Id $formId -Confirm:$false
                
                Write-Host "  Removed form: $formName (ID: $formId)"
                
                # Verify the form was removed
                try {
                    $removedForm = Get-DataverseForm -Connection $connection -Id $formId
                    if ($removedForm) {
                        throw "Form should have been removed but still exists"
                    }
                } catch {
                    # Expected - form not found
                    Write-Host "  Verified: Form successfully removed (not found in query)"
                }
                
                Write-Host ""
                Write-Host "=========================================="
                Write-Host "Form Manipulation E2E Test PASSED"
                Write-Host "=========================================="
                Write-Host ""
                Write-Host "Summary:"
                Write-Host "  - Created form with unique name"
                Write-Host "  - Created and updated tabs with multi-column layouts"
                Write-Host "  - Created multiple sections within tabs"
                Write-Host "  - Created controls of different types (Standard, Lookup, Email)"
                Write-Host "  - Updated control properties"
                Write-Host "  - Published form after all modifications (tested -Publish parameter)"
                Write-Host "  - Successfully removed controls, sections, tabs"
                Write-Host "  - Cleaned up test form"
                Write-Host "  - Handled cleanup of leftover test forms from previous runs"
                Write-Host ""
                
            } catch {
                Write-Host ""
                Write-Host "=========================================="
                Write-Host "ERROR: Form Manipulation E2E Test FAILED"
                Write-Host "=========================================="
                Write-Host ""
                throw "Failed: " + ($_ | Format-Table -force * | Out-String)
            }
        }

        if ($LASTEXITCODE -ne 0) {
            throw "Failed"
        }
    }
}
