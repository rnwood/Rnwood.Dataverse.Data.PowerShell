using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.OrganizationSettings
{
    /// <summary>
    /// Organization settings manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/OrganizationSettings.Tests.ps1
    /// </summary>
    public class OrganizationSettingsTests : E2ETestBase
    {
        [Fact]
        public void CanGetAndUpdateOrganizationSettingsAndOrgDbOrgSettings()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    Write-Host '========================================='
    Write-Host 'Testing Organization Table Columns'
    Write-Host '========================================='
    
    # Get current organization record
    Write-Host 'Getting organization record...'
    $org = Get-DataverseOrganizationSettings -Connection $connection
    
    if (-not $org) {
        throw 'No organization record returned'
    }
    
    Write-Host ""Organization name: $($org.name)""
    Write-Host ""Organization ID: $($org.Id)""
    
    # Test updating MaximumTrackingNumber (integer column)
    $originalTrackingNumber = $org.maximumtrackingnumber
    Write-Host ""Original MaximumTrackingNumber: $originalTrackingNumber""
    
    # Calculate new value (increment by 1, or set to 1 if null)
    $newTrackingNumber = if ($null -eq $originalTrackingNumber) { 1 } else { $originalTrackingNumber + 1 }
    Write-Host ""Setting MaximumTrackingNumber to: $newTrackingNumber""
    
    # Update the value
    Set-DataverseOrganizationSettings -Connection $connection -InputObject @{
        maximumtrackingnumber = $newTrackingNumber
    } -Confirm:$false -Verbose
    
    # Verify the change
    $updatedOrg = Get-DataverseOrganizationSettings -Connection $connection
    if ($updatedOrg.maximumtrackingnumber -ne $newTrackingNumber) {
        throw ""MaximumTrackingNumber was not updated. Expected: $newTrackingNumber, Actual: $($updatedOrg.maximumtrackingnumber)""
    }
    Write-Host ""MaximumTrackingNumber successfully updated to: $($updatedOrg.maximumtrackingnumber)""
    
    # Restore original value
    Write-Host ""Restoring original MaximumTrackingNumber: $originalTrackingNumber""
    Set-DataverseOrganizationSettings -Connection $connection -InputObject @{
        maximumtrackingnumber = $originalTrackingNumber
    } -Confirm:$false -Verbose
    
    Write-Host ''
    Write-Host '========================================='
    Write-Host 'Testing OrgDbOrgSettings XML'
    Write-Host '========================================='
    
    # Get current OrgDbOrgSettings
    Write-Host 'Getting OrgDbOrgSettings...'
    $currentSettings = Get-DataverseOrganizationSettings -Connection $connection -OrgDbOrgSettings
    
    if ($null -eq $currentSettings) {
        throw 'No OrgDbOrgSettings returned'
    }
    
    $settingCount = ($currentSettings.PSObject.Properties | Measure-Object).Count
    Write-Host ""Found $settingCount parsed settings""
    
    # Test updating AllowSaveAsDraftAppointment
    $originalValue = $currentSettings.AllowSaveAsDraftAppointment
    Write-Host ""Original AllowSaveAsDraftAppointment: $originalValue""
    
    # Toggle the boolean value
    $newValue = -not [bool]$originalValue
    Write-Host ""Setting AllowSaveAsDraftAppointment to: $newValue""
    
    # Update the setting
    Set-DataverseOrganizationSettings -Connection $connection -InputObject @{
        AllowSaveAsDraftAppointment = $newValue
    } -OrgDbOrgSettings -Confirm:$false -Verbose
    
    # Verify the change
    $updatedSettings = Get-DataverseOrganizationSettings -Connection $connection -OrgDbOrgSettings
    if ($updatedSettings.AllowSaveAsDraftAppointment -ne $newValue) {
        throw ""AllowSaveAsDraftAppointment was not updated. Expected: $newValue, Actual: $($updatedSettings.AllowSaveAsDraftAppointment)""
    }
    Write-Host ""AllowSaveAsDraftAppointment successfully updated to: $($updatedSettings.AllowSaveAsDraftAppointment)""
    
    # Restore original value
    Write-Host ""Restoring original AllowSaveAsDraftAppointment: $originalValue""
    Set-DataverseOrganizationSettings -Connection $connection -InputObject @{
        AllowSaveAsDraftAppointment = $originalValue
    } -OrgDbOrgSettings -Confirm:$false -Verbose
    
    # Final verification
    $finalSettings = Get-DataverseOrganizationSettings -Connection $connection -OrgDbOrgSettings
    
    # When restoring to null/empty, the setting may be removed from XML and return as $null or False
    $finalValue = $finalSettings.AllowSaveAsDraftAppointment
    $valuesMatch = if ([string]::IsNullOrEmpty($originalValue)) {
        # If original was null/empty, accept null, empty, or False as valid restoration
        [string]::IsNullOrEmpty($finalValue) -or $finalValue -eq $false
    } else {
        $finalValue -eq $originalValue
    }
    
    if (-not $valuesMatch) {
        throw ""Failed to restore original AllowSaveAsDraftAppointment value. Expected: '$originalValue', Got: '$finalValue'""
    }
    Write-Host ""AllowSaveAsDraftAppointment successfully restored (original: '$originalValue', final: '$finalValue')""
    
    Write-Host '✓ All organization settings operations completed successfully'
    Write-Host 'SUCCESS: All organization settings operations completed'
}
catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS");
        }
    }
}
