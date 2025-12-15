$ErrorActionPreference = "Stop"

Describe "Organization Settings E2E Tests" -Skip {

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

        Import-Module Rnwood.Dataverse.Data.PowerShell
    }

    It "Can get and update organization settings and OrgDbOrgSettings" {
        $ErrorActionPreference = "Stop"
        $ConfirmPreference = 'None'
        $VerbosePreference = 'Continue'

        try {
            $connection = Get-DataverseConnection -url ${env:E2ETESTS_URL} -ClientId ${env:E2ETESTS_CLIENTID} -ClientSecret ${env:E2ETESTS_CLIENTSECRET}
            $connection.EnableAffinityCookie = $true

            Write-Host "========================================="
            Write-Host "Testing Organization Table Columns"
            Write-Host "========================================="
            
            # Get current organization record
            Write-Host "Getting organization record..."
            $org = Get-DataverseOrganizationSettings -Connection $connection
            
            if (-not $org) {
                throw "No organization record returned"
            }
            
            Write-Host "Organization name: $($org.name)"
            Write-Host "Organization ID: $($org.Id)"
            
            # Test updating MaximumTrackingNumber (integer column)
            $originalTrackingNumber = $org.maximumtrackingnumber
            Write-Host "Original MaximumTrackingNumber: $originalTrackingNumber"
            
            # Calculate new value (increment by 1, or set to 1 if null)
            $newTrackingNumber = if ($null -eq $originalTrackingNumber) { 1 } else { $originalTrackingNumber + 1 }
            Write-Host "Setting MaximumTrackingNumber to: $newTrackingNumber"
            
            # Update the value
            Set-DataverseOrganizationSettings -Connection $connection -InputObject @{
                maximumtrackingnumber = $newTrackingNumber
            } -Confirm:$false -Verbose
            
            # Verify the change
            $updatedOrg = Get-DataverseOrganizationSettings -Connection $connection
            if ($updatedOrg.maximumtrackingnumber -ne $newTrackingNumber) {
                throw "MaximumTrackingNumber was not updated. Expected: $newTrackingNumber, Actual: $($updatedOrg.maximumtrackingnumber)"
            }
            Write-Host "MaximumTrackingNumber successfully updated to: $($updatedOrg.maximumtrackingnumber)"
            
            # Restore original value
            Write-Host "Restoring original MaximumTrackingNumber: $originalTrackingNumber"
            Set-DataverseOrganizationSettings -Connection $connection -InputObject @{
                maximumtrackingnumber = $originalTrackingNumber
            } -Confirm:$false -Verbose
            
            Write-Host ""
            Write-Host "========================================="
            Write-Host "Testing OrgDbOrgSettings XML"
            Write-Host "========================================="
            
            # Get current OrgDbOrgSettings
            Write-Host "Getting OrgDbOrgSettings..."
            $currentSettings = Get-DataverseOrganizationSettings -Connection $connection -OrgDbOrgSettings
            
            if ($null -eq $currentSettings) {
                throw "No OrgDbOrgSettings returned"
            }
            
            $settingCount = ($currentSettings.PSObject.Properties | Measure-Object).Count
            Write-Host "Found $settingCount parsed settings"
            
            # Test updating AllowSaveAsDraftAppointment
            $originalValue = $currentSettings.AllowSaveAsDraftAppointment
            Write-Host "Original AllowSaveAsDraftAppointment: $originalValue"
            
            # Toggle the boolean value
            $newValue = -not [bool]$originalValue
            Write-Host "Setting AllowSaveAsDraftAppointment to: $newValue"
            
            # Update the setting
            Set-DataverseOrganizationSettings -Connection $connection -InputObject @{
                AllowSaveAsDraftAppointment = $newValue
            } -OrgDbOrgSettings -Confirm:$false -Verbose
            
            # Verify the change
            $updatedSettings = Get-DataverseOrganizationSettings -Connection $connection -OrgDbOrgSettings
            if ($updatedSettings.AllowSaveAsDraftAppointment -ne $newValue) {
                throw "AllowSaveAsDraftAppointment was not updated. Expected: $newValue, Actual: $($updatedSettings.AllowSaveAsDraftAppointment)"
            }
            Write-Host "AllowSaveAsDraftAppointment successfully updated to: $($updatedSettings.AllowSaveAsDraftAppointment)"
            
            # Restore original value
            Write-Host "Restoring original AllowSaveAsDraftAppointment: $originalValue"
            Set-DataverseOrganizationSettings -Connection $connection -InputObject @{
                AllowSaveAsDraftAppointment = $originalValue
            } -OrgDbOrgSettings -Confirm:$false -Verbose
            
            # Final verification
            $finalSettings = Get-DataverseOrganizationSettings -Connection $connection -OrgDbOrgSettings
            if ($finalSettings.AllowSaveAsDraftAppointment -ne $originalValue) {
                throw "Failed to restore original AllowSaveAsDraftAppointment value"
            }
            Write-Host "AllowSaveAsDraftAppointment successfully restored to: $($finalSettings.AllowSaveAsDraftAppointment)"
            
            Write-Host ""
            Write-Host "========================================="
            Write-Host "All tests passed successfully!"
            Write-Host "========================================="
            
        } catch {
            Write-Error "Test failed: $_"
            Write-Error $_.ScriptStackTrace
            throw
        }
    }
}
