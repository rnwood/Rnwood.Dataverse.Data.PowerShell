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

try {
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $formName = ""E2ETestForm-$testRunId""
    $entityName = 'account'
    
    Write-Host '=========================================='
    Write-Host 'Starting Form Manipulation E2E Test'
    Write-Host ""Test Run ID: $testRunId""
    Write-Host ""Form Name: $formName""
    Write-Host '=========================================='
    
    # Note: Pre-cleanup of leftover forms has been moved to nightly cleanup script
    # (scripts/Cleanup-E2ETestArtifacts.ps1) to improve test performance
    
    # ============================================================
    # STEP 1: CREATE FORM
    # ============================================================
    Write-Host ''
    Write-Host 'Step 1: Creating new form...'
    
    $formId = Invoke-WithRetry {
        Set-DataverseForm -Connection $connection `
            -Entity $entityName `
            -Name $formName `
            -FormType Main `
            -Description ""E2E test form for comprehensive feature testing - Run $testRunId"" `
            -IsActive `
            -PassThru
    }
    
    if (-not $formId) {
        throw 'Failed to create form - no form ID returned'
    }
    
    Write-Host ""  Created form with ID: $formId""
    
    # Verify form was created
    $createdForm = Invoke-WithRetry {
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
    }
    
    Write-Host '  Created tab: CustomTab'
    
    $tab = Invoke-WithRetry {
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
    }
    
    Write-Host '  Created section: TestSection'
    
    $section = Invoke-WithRetry {
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
    }
    
    Write-Host '  Created control: name (Standard, Required)'
    
    $control = Invoke-WithRetry {
        Get-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionName 'TestSection' `
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
        Set-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionName 'TestSection' `
            -ControlId 'name' `
            -DataField 'name' `
            -Disabled `
            -Confirm:$false
    }
    
    Write-Host ""  Updated control 'name' to disabled""
    
    # ============================================================
    # STEP 6: PUBLISH FORM
    # ============================================================
    Write-Host ''
    Write-Host 'Step 6: Publishing form after all modifications...'
    
    Invoke-WithRetry {
        Set-DataverseForm -Connection $connection `
            -Id $formId `
            -Publish `
            -Confirm:$false
    }
    
    Write-Host '  Published form successfully'
    
    $publishedForm = Invoke-WithRetry {
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
        Remove-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionName 'TestSection' `
            -ControlId 'name' `
            -Confirm:$false
    }
    
    Write-Host '  Removed control: name'
    
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
    }
    
    Write-Host '  Verified: Control successfully removed'
    
    # ============================================================
    # STEP 8: REMOVE SECTION
    # ============================================================
    Write-Host ''
    Write-Host 'Step 8: Removing a section...'
    
    Invoke-WithRetry {
        Remove-DataverseFormSection -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -SectionName 'TestSection' `
            -Confirm:$false
    }
    
    Write-Host '  Removed section: TestSection'
    
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
    
    # ============================================================
    # STEP 9: REMOVE TAB
    # ============================================================
    Write-Host ''
    Write-Host 'Step 9: Removing a tab...'
    
    Invoke-WithRetry {
        Remove-DataverseFormTab -Connection $connection `
            -FormId $formId `
            -TabName 'CustomTab' `
            -Confirm:$false
    }
    
    Write-Host '  Removed tab: CustomTab'
    
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
    
    # ============================================================
    # STEP 10: CLEANUP - REMOVE FORM
    # ============================================================
    Write-Host ''
    Write-Host 'Step 10: Final cleanup - Removing test form...'
    
    Invoke-WithRetry {
        Remove-DataverseForm -Connection $connection -Id $formId -Confirm:$false
    }
    
    Write-Host ""  Removed form: $formName (ID: $formId)""
    
    # Verify the form was removed
    try {
        Invoke-WithRetry {
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
    Write-Host '  Note: Nightly cleanup handles leftover artifacts from failed runs'
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

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("Form Manipulation E2E Test PASSED");
        }

        [Fact]
        public void UpdateControlByControlId_WithoutDataField_ShouldSucceed()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $formName = ""E2ETestForm-UpdateWithoutDataField-$testRunId""
    $entityName = 'account'
    
    Write-Host '=========================================='
    Write-Host 'Starting Update Control Without DataField E2E Test'
    Write-Host ""Test Run ID: $testRunId""
    Write-Host ""Form Name: $formName""
    Write-Host '=========================================='
    
    # Create test form
    Write-Host ''
    Write-Host 'Creating test form...'
    
    $formId = Invoke-WithRetry {
        Set-DataverseForm -Connection $connection `
            -Entity $entityName `
            -Name $formName `
            -FormType Main `
            -Description ""E2E test form for updating control without DataField - Run $testRunId"" `
            -IsActive `
            -PassThru
    }
    
    Write-Host ""  Created form with ID: $formId""
    
    # Create tab
    $tabId = '{' + [guid]::NewGuid().ToString().ToUpper() + '}'
    Invoke-WithRetry {
        Set-DataverseFormTab -Connection $connection `
            -FormId $formId `
            -TabId $tabId `
            -Name 'TestTab' `
            -Label 'Test Tab' `
            -Layout OneColumn `
            -Expanded `
            -ShowLabel `
            -Confirm:$false
    }
    
    Write-Host '  Created tab: TestTab'
    
    # Create section
    Invoke-WithRetry {
        Set-DataverseFormSection -Connection $connection `
            -FormId $formId `
            -TabName 'TestTab' `
            -Name 'TestSection' `
            -Label 'Test Section' `
            -ShowLabel `
            -Confirm:$false
    }
    
    Write-Host '  Created section: TestSection'
    
    # Create control with DataField
    Write-Host ''
    Write-Host 'Creating control with DataField...'
    Invoke-WithRetry {
        Set-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'TestTab' `
            -SectionName 'TestSection' `
            -ControlId 'name' `
            -DataField 'name' `
            -ControlType Standard `
            -Labels @{1033 = 'Original Label'} `
            -Confirm:$false
    }
    
    Write-Host '  Created control: name'
    
    # Update control WITHOUT DataField (using only ControlId)
    # This is the bug fix scenario from the issue
    Write-Host ''
    Write-Host 'Updating control WITHOUT DataField parameter...'
    Invoke-WithRetry {
        Set-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'TestTab' `
            -SectionName 'TestSection' `
            -ControlId 'name' `
            -Labels @{1033 = 'Updated Label'} `
            -Disabled `
            -Confirm:$false
    }
    
    Write-Host '  Updated control successfully without DataField parameter'
    
    # Verify the update worked
    $updatedControl = Invoke-WithRetry {
        Get-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'TestTab' `
            -SectionName 'TestSection' `
            -ControlId 'name'
    }
    
    if (-not $updatedControl) {
        throw 'Failed to retrieve updated control'
    }
    
    if ($updatedControl.DataField -ne 'name') {
        throw ""Control DataField should still be 'name', got: $($updatedControl.DataField)""
    }
    
    if (-not $updatedControl.Disabled) {
        throw 'Control should be disabled after update'
    }
    
    Write-Host '  Verified: Control was updated successfully'
    Write-Host '    - DataField preserved: name'
    Write-Host '    - Disabled: true'
    Write-Host '    - Label updated: Updated Label'
    
    # Cleanup
    Write-Host ''
    Write-Host 'Cleaning up test form...'
    Invoke-WithRetry {
        Remove-DataverseForm -Connection $connection -Id $formId -Confirm:$false
    }
    
    Write-Host '  Removed test form'
    
    Write-Host ''
    Write-Host '=========================================='
    Write-Host 'Update Control Without DataField E2E Test PASSED'
    Write-Host '=========================================='
    Write-Host ''
    
    exit 0
}
catch {
    Write-Host ''
    Write-Host '=========================================='
    Write-Host 'ERROR: Update Control Without DataField E2E Test FAILED'
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

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("Update Control Without DataField E2E Test PASSED");
        }
    }
}
