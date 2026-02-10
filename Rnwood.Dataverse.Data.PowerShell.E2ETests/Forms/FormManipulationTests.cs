using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Forms
{
    /// <summary>
    /// Form manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/FormManipulation.Tests.ps1
    /// </summary>
    public class FormManipulationTests : E2ETestBase
    {
        [Fact]
        public void ComprehensivelyExercisesAllFormManipulationFeaturesWithCleanup()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

<<<<<<< HEAD
try {
=======
function Invoke-WithRetry {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$ScriptBlock,
        [int]$MaxRetries = 5,
        [int]$InitialDelaySeconds = 10
    )

    $attempt = 0
    $delay = $InitialDelaySeconds

    while ($attempt -lt $MaxRetries) {
        try {
            $attempt++
            Write-Verbose ""Attempt $attempt of $MaxRetries""
            & $ScriptBlock
            return
        }
        catch {
            if ($_.Exception.Message -like '*Cannot start the requested operation*EntityCustomization*') {
                Write-Warning 'EntityCustomization operation conflict. Waiting 2 minutes...'
                $attempt--
                Start-Sleep -Seconds 120
                continue
            }
            
            if ($attempt -eq $MaxRetries) {
                throw
            }

            Write-Warning ""Attempt $attempt failed: $_. Retrying in $delay seconds...""
            Start-Sleep -Seconds $delay
            $delay = $delay * 2
        }
    }
}

try {
    $connection.EnableAffinityCookie = $true
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $formName = ""E2ETestForm-$testRunId""
    $entityName = 'account'
    
    Write-Host '=========================================='
    Write-Host 'Starting Form Manipulation E2E Test'
    Write-Host ""Test Run ID: $testRunId""
    Write-Host ""Form Name: $formName""
    Write-Host '=========================================='
    
<<<<<<< HEAD
    # Note: Pre-cleanup of leftover forms has been moved to nightly cleanup script
    # (scripts/Cleanup-E2ETestArtifacts.ps1) to improve test performance
=======
    # ============================================================
    # CLEANUP: Remove any leftover forms from previous failed runs
    # ============================================================
    Write-Host ''
    Write-Host 'Step 0: Cleanup - Removing any leftover test forms from previous runs...'
    
    Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        $script:existingTestForms = Get-DataverseRecord -Connection $connection -TableName systemform -FilterValues @{
            'name:Like' = 'E2ETestForm-%'
            'objecttypecode' = $entityName
        } -Columns formid, name
        
        if ($script:existingTestForms -and $script:existingTestForms.Count -gt 0) {
            Write-Host ""  Found $($existingTestForms.Count) leftover test form(s) to clean up""
            foreach ($oldForm in $existingTestForms) {
                Write-Host ""  Deleting form: $($oldForm.name) (ID: $($oldForm.formid))""
                Invoke-WithRetry {
                    Wait-DataversePublish -Connection $connection -Verbose
                    Remove-DataverseForm -Connection $connection -Id $oldForm.formid -Confirm:`$false
                }
            }
            Write-Host '  Cleanup complete'
        }
        else {
            Write-Host '  No leftover test forms found'
        }
    }
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    
    # ============================================================
    # STEP 1: CREATE FORM
    # ============================================================
    Write-Host ''
    Write-Host 'Step 1: Creating new form...'
    
    $formId = Invoke-WithRetry {
<<<<<<< HEAD
        Set-DataverseForm -Connection $connection `
            -Entity $entityName `
            -Name $formName `
            -FormType Main `
            -Description ""E2E test form for comprehensive feature testing - Run $testRunId"" `
            -IsActive `
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Set-DataverseForm -Connection $connection ``
            -Entity $entityName ``
            -Name $formName ``
            -FormType Main ``
            -Description ""E2E test form for comprehensive feature testing - Run $testRunId"" ``
            -IsActive ``
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
            -PassThru
    }
    
    if (-not $formId) {
        throw 'Failed to create form - no form ID returned'
    }
    
    Write-Host ""  Created form with ID: $formId""
    
    # Verify form was created
    $createdForm = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseForm -Connection $connection -Id $formId
    }
    if (-not $createdForm) {
        throw 'Failed to retrieve created form'
    }
    
    if ($createdForm.name -ne $formName) {
        throw ""Form name mismatch. Expected: $formName, Got: $($createdForm.name)""
    }
    
    Write-Host '  Verified: Form created successfully with correct properties'
    
    # ============================================================
    # STEP 2: CREATE AND MANIPULATE TABS
    # ============================================================
    Write-Host ''
    Write-Host 'Step 2: Creating and manipulating tabs...'
    
    $newTabId = '{' + [guid]::NewGuid().ToString().ToUpper() + '}'
    Write-Host ""  Creating new tab with ID: $newTabId""
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Set-DataverseFormTab -Connection $connection `
            -FormId $formId `
            -TabId $newTabId `
            -Name 'CustomTab' `
            -Label 'Custom Tab' `
            -Layout TwoColumns `
            -Column1Width 60 `
            -Column2Width 40 `
            -Expanded `
            -ShowLabel `
            -Confirm:$false
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Set-DataverseFormTab -Connection $connection ``
            -FormId $formId ``
            -TabId $newTabId ``
            -Name 'CustomTab' ``
            -Label 'Custom Tab' ``
            -Layout TwoColumns ``
            -Column1Width 60 ``
            -Column2Width 40 ``
            -Expanded ``
            -ShowLabel ``
            -Confirm:`$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host '  Created tab: CustomTab'
    
    $tab = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseFormTab -Connection $connection -FormId $formId -TabName 'CustomTab'
    }
    
    if (-not $tab) {
        throw 'Failed to retrieve created tab'
    }
    
    if ($tab.Layout -ne 'TwoColumns') {
        throw ""Tab layout mismatch. Expected: TwoColumns, Got: $($tab.Layout)""
    }
    
    Write-Host ""  Verified: Tab created with TwoColumns layout ($($tab.Column1Width)% / $($tab.Column2Width)%)""
    
    # Update tab layout to three columns
    Write-Host '  Updating tab layout to ThreeColumns...'
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Set-DataverseFormTab -Connection $connection `
            -FormId $formId `
            -TabId $newTabId `
            -Layout ThreeColumns `
            -Column1Width 33 `
            -Column2Width 34 `
            -Column3Width 33 `
            -Confirm:$false
    }
    
    $updatedTab = Invoke-WithRetry {
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Set-DataverseFormTab -Connection $connection ``
            -FormId $formId ``
            -TabId $newTabId ``
            -Layout ThreeColumns ``
            -Column1Width 33 ``
            -Column2Width 34 ``
            -Column3Width 33 ``
            -Confirm:`$false
    }
    
    $updatedTab = Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseFormTab -Connection $connection -FormId $formId -TabName 'CustomTab'
    }
    
    if ($updatedTab.Layout -ne 'ThreeColumns') {
        throw ""Tab layout update failed. Expected: ThreeColumns, Got: $($updatedTab.Layout)""
    }
    
    Write-Host ""  Verified: Tab layout updated to ThreeColumns ($($updatedTab.Column1Width)% / $($updatedTab.Column2Width)% / $($updatedTab.Column3Width)%)""
    
    # ============================================================
    # STEP 3: CREATE AND MANIPULATE SECTIONS
    # ============================================================
    Write-Host ''
    Write-Host 'Step 3: Creating and manipulating sections...'
    
    $newSectionId = '{' + [guid]::NewGuid().ToString().ToUpper() + '}'
    Write-Host ""  Creating section with ID: $newSectionId""
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Set-DataverseFormSection -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionId $newSectionId `
            -Name 'TestSection' `
            -Label 'Test Section' `
            -ShowLabel `
            -ShowBar `
            -Columns 2 `
            -Confirm:$false
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Set-DataverseFormSection -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab' ``
            -SectionId $newSectionId ``
            -Name 'TestSection' ``
            -Label 'Test Section' ``
            -ShowLabel ``
            -ShowBar ``
            -Columns 2 ``
            -Confirm:`$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host '  Created section: TestSection'
    
    $section = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseFormSection -Connection $connection -FormId $formId -TabName 'CustomTab' -SectionName 'TestSection'
    }
    
    if (-not $section) {
        throw 'Failed to retrieve created section'
    }
    
    Write-Host '  Verified: Section created successfully'
    
    # ============================================================
    # STEP 4: CREATE CONTROLS
    # ============================================================
    Write-Host ''
    Write-Host 'Step 4: Creating controls...'
    
    Write-Host ""  Creating text control for 'name' field...""
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Set-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionName 'TestSection' `
            -ControlId 'name' `
            -DataField 'name' `
            -ControlType Standard `
            -Labels @{1033 = 'Account Name'} `
            -IsRequired `
            -Confirm:$false
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Set-DataverseFormControl -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab' ``
            -SectionName 'TestSection' ``
            -ControlId 'name' ``
            -DataField 'name' ``
            -ControlType Standard ``
            -Label 'Account Name' ``
            -IsRequired ``
            -Confirm:`$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host '  Created control: name (Standard, Required)'
    
    $control = Invoke-WithRetry {
<<<<<<< HEAD
        Get-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionName 'TestSection' `
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Get-DataverseFormControl -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab' ``
            -SectionName 'TestSection' ``
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
            -ControlId 'name'
    }
    
    if (-not $control) {
        throw ""Failed to retrieve created control 'name'""
    }
    
    if ($control.DataField -ne 'name') {
        throw ""Control DataField mismatch. Expected: name, Got: $($control.DataField)""
    }
    
    Write-Host ""  Verified: Control 'name' created successfully""
    
    # ============================================================
    # STEP 5: UPDATE CONTROL PROPERTIES
    # ============================================================
    Write-Host ''
    Write-Host 'Step 5: Updating control properties...'
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Set-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionName 'TestSection' `
            -ControlId 'name' `
            -DataField 'name' `
            -Disabled `
            -Confirm:$false
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Set-DataverseFormControl -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab' ``
            -SectionName 'TestSection' ``
            -ControlId 'name' ``
            -Disabled ``
            -Confirm:`$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host ""  Updated control 'name' to disabled""
    
    # ============================================================
    # STEP 6: PUBLISH FORM
    # ============================================================
    Write-Host ''
    Write-Host 'Step 6: Publishing form after all modifications...'
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Set-DataverseForm -Connection $connection `
            -Id $formId `
            -Publish `
            -Confirm:$false
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Set-DataverseForm -Connection $connection ``
            -Id $formId ``
            -Publish ``
            -Confirm:`$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host '  Published form successfully'
    
    $publishedForm = Invoke-WithRetry {
<<<<<<< HEAD
=======
        Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
        Get-DataverseForm -Connection $connection -Id $formId -IncludeFormXml
    }
    if (-not $publishedForm) {
        throw 'Failed to retrieve form after publish'
    }
    
    if (-not $publishedForm.formxml) {
        throw 'Form XML is empty after publish'
    }
    
    Write-Host '  Verified: Form published successfully and formxml is available'
    
    # ============================================================
    # STEP 7: REMOVE CONTROL
    # ============================================================
    Write-Host ''
    Write-Host 'Step 7: Removing a control...'
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Remove-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionName 'TestSection' `
            -ControlId 'name' `
            -Confirm:$false
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Remove-DataverseFormControl -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab' ``
            -SectionName 'TestSection' ``
            -ControlId 'name' ``
            -Confirm:`$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host '  Removed control: name'
    
<<<<<<< HEAD
    try {
        $removedControl = Invoke-WithRetry {
            Get-DataverseFormControl -Connection $connection `
                -FormId $formId `
                -TabName 'CustomTab' `
                -SectionName 'TestSection' `
                -ControlId 'name'
        }
        
        if ($removedControl) {
            throw ""Control 'name' should have been removed but still exists""
        }
    }
    catch {
        # Expected - control not found after removal
        Write-Host '  Verified: Control successfully removed (not found in query)'
=======
    $removedControl = Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        Get-DataverseFormControl -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab' ``
            -SectionName 'TestSection' ``
            -ControlId 'name'
    }
    
    if ($removedControl) {
        throw ""Control 'name' should have been removed but still exists""
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host '  Verified: Control successfully removed'
    
    # ============================================================
    # STEP 8: REMOVE SECTION
    # ============================================================
    Write-Host ''
    Write-Host 'Step 8: Removing a section...'
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Remove-DataverseFormSection -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionName 'TestSection' `
            -Confirm:$false
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Remove-DataverseFormSection -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab' ``
            -SectionName 'TestSection' ``
            -Confirm:`$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host '  Removed section: TestSection'
    
<<<<<<< HEAD
    try {
        $removedSection = Invoke-WithRetry {
            Get-DataverseFormSection -Connection $connection `
                -FormId $formId `
                -TabName 'CustomTab' `
                -SectionName 'TestSection'
        }
        
        if ($removedSection) {
            throw ""Section 'TestSection' should have been removed but still exists""
        }
    }
    catch {
        # Expected - section not found after removal
        Write-Host '  Verified: Section successfully removed (not found in query)'
    }
    
=======
    $removedSection = Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        Get-DataverseFormSection -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab' ``
            -SectionName 'TestSection'
    }
    
    if ($removedSection) {
        throw ""Section 'TestSection' should have been removed but still exists""
    }
    
    Write-Host '  Verified: Section successfully removed'
    
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    # ============================================================
    # STEP 9: REMOVE TAB
    # ============================================================
    Write-Host ''
    Write-Host 'Step 9: Removing a tab...'
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Remove-DataverseFormTab -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -Confirm:$false
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Remove-DataverseFormTab -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab' ``
            -Confirm:`$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host '  Removed tab: CustomTab'
    
<<<<<<< HEAD
    try {
        $removedTab = Invoke-WithRetry {
            Get-DataverseFormTab -Connection $connection `
                -FormId $formId `
                -TabName 'CustomTab'
        }
        
        if ($removedTab) {
            throw ""Tab 'CustomTab' should have been removed but still exists""
        }
    }
    catch {
        # Expected - tab not found after removal
        Write-Host '  Verified: Tab successfully removed (not found in query)'
    }
    
=======
    $removedTab = Invoke-WithRetry {
        Wait-DataversePublish -Connection $connection -Verbose
        Get-DataverseFormTab -Connection $connection ``
            -FormId $formId ``
            -TabName 'CustomTab'
    }
    
    if ($removedTab) {
        throw ""Tab 'CustomTab' should have been removed but still exists""
    }
    
    Write-Host '  Verified: Tab successfully removed'
    
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    # ============================================================
    # STEP 10: CLEANUP - REMOVE FORM
    # ============================================================
    Write-Host ''
    Write-Host 'Step 10: Final cleanup - Removing test form...'
    
    Invoke-WithRetry {
<<<<<<< HEAD
        Remove-DataverseForm -Connection $connection -Id $formId -Confirm:$false
=======
        Wait-DataversePublish -Connection $connection -Verbose
        Remove-DataverseForm -Connection $connection -Id $formId -Confirm:`$false
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    }
    
    Write-Host ""  Removed form: $formName (ID: $formId)""
    
    # Verify the form was removed
    try {
        Invoke-WithRetry {
<<<<<<< HEAD
=======
            Wait-DataversePublish -Connection $connection -Verbose
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
            $removedForm = Get-DataverseForm -Connection $connection -Id $formId
            if ($removedForm) {
                throw 'Form should have been removed but still exists'
            }
        }
    }
    catch {
        # Expected - form not found
        Write-Host '  Verified: Form successfully removed (not found in query)'
    }
    
    Write-Host ''
    Write-Host '=========================================='
    Write-Host 'Form Manipulation E2E Test PASSED'
    Write-Host '=========================================='
    Write-Host ''
    Write-Host 'Summary:'
    Write-Host '  - Created form with unique name'
    Write-Host '  - Created and updated tabs with multi-column layouts'
    Write-Host '  - Created sections within tabs'
    Write-Host '  - Created controls (Standard type)'
    Write-Host '  - Updated control properties'
    Write-Host '  - Published form after all modifications'
    Write-Host '  - Successfully removed controls, sections, tabs'
    Write-Host '  - Cleaned up test form'
<<<<<<< HEAD
    Write-Host '  Note: Nightly cleanup handles leftover artifacts from failed runs'
=======
    Write-Host '  - Handled cleanup of leftover test forms from previous runs'
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)
    Write-Host ''
    
}
catch {
    Write-Host ''
    Write-Host '=========================================='
    Write-Host 'ERROR: Form Manipulation E2E Test FAILED'
    Write-Host '=========================================='
    Write-Host ''
    Write-Host 'Error Details:'
    Write-Host ""  Message: $($_.Exception.Message)""
    Write-Host ""  Script Line: $($_.InvocationInfo.ScriptLineNumber)""
    Write-Host ''
    Write-Host 'Full Error:'
    Write-Host ($_ | Out-String)
    Write-Host ''
    throw
}
");

<<<<<<< HEAD
            var result = RunScript(script);
=======
            var result = RunScript(script, timeoutSeconds: 1200); // 20 minute timeout for comprehensive test
>>>>>>> df047b13 (tests: migrate e2e tests to xunit)

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("Form Manipulation E2E Test PASSED");
        }
    }
}
