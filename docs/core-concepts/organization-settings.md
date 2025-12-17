
# Organization Settings

The organization settings cmdlets allow you to retrieve and update settings for the Dataverse environment. These settings control various behaviors and features of the environment.

## Overview

Every Dataverse environment has a single organization record that contains environment-wide settings. This module provides cmdlets to work with both:

1. **Organization table columns** - Standard table fields like `name`, `maximumtrackingnumber`, etc.
2. **OrgDbOrgSettings XML** - Additional settings stored in an XML column for more advanced configuration

The cmdlets use the `-OrgDbOrgSettings` switch parameter to cleanly separate these two modes of operation.

## Getting Organization Settings

### Get Full Organization Record

Retrieve all organization table columns:

```powershell
# Get the organization record (without -OrgDbOrgSettings)
$org = Get-DataverseOrganizationSettings -Connection $conn

# Access properties
$org.name                          # Organization name
$org.organizationid                # Organization ID (GUID)
$org.maximumtrackingnumber         # Maximum tracking number
$org.currencysymbol               # Currency symbol
```

### Get OrgDbOrgSettings Only

Retrieve only the parsed OrgDbOrgSettings as typed properties:

```powershell
# Get OrgDbOrgSettings (with -OrgDbOrgSettings switch)
$settings = Get-DataverseOrganizationSettings -Connection $conn -OrgDbOrgSettings

# Access parsed settings with typed values
$settings.MaxUploadFileSize              # Integer
$settings.EnableBingMapsIntegration      # Boolean
$settings.AllowSaveAsDraftAppointment    # Boolean
```

The XML settings are automatically parsed and typed as boolean, integer, or string values for easy use.

### Include Raw XML

When getting the full organization record, you can optionally include the raw XML:

```powershell
# Include the raw orgdborgsettings XML column
$org = Get-DataverseOrganizationSettings -Connection $conn -IncludeRawXml

# View the raw XML
$org.orgdborgsettings
```

**Note**: The `-IncludeRawXml` parameter is ignored when using `-OrgDbOrgSettings`.

## Updating Organization Settings

The Set cmdlet compares existing values with new values and only updates what has changed. Use the `-Verbose` parameter to see what's being changed.

### Update Organization Table Columns

Update standard organization table fields:

```powershell
# Update organization table columns
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    maximumtrackingnumber = 100000
} -Confirm:$false -Verbose
```

**Verbose Output Example**:
```
VERBOSE: Column 'maximumtrackingnumber': Changing from '50000' to '100000'
VERBOSE: Updated 1 attribute(s) in organization record
```

### Update OrgDbOrgSettings

Update settings in the OrgDbOrgSettings XML:

```powershell
# Update OrgDbOrgSettings XML settings
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    MaxUploadFileSize = 10485760
    EnableBingMapsIntegration = $true
    AllowSaveAsDraftAppointment = $false
} -OrgDbOrgSettings -Confirm:$false -Verbose
```

**Verbose Output Example**:
```
VERBOSE: Setting 'MaxUploadFileSize': Changing from '5242880' to '10485760'
VERBOSE: Setting 'EnableBingMapsIntegration': No change (value is 'true')
VERBOSE: Setting 'AllowSaveAsDraftAppointment': Changing from 'true' to 'false'
VERBOSE: Updated 1 attribute(s) in organization record
```

### Remove OrgDbOrgSettings

Remove a setting from the OrgDbOrgSettings XML by passing `$null`:

```powershell
# Remove a setting by passing $null
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    ObsoleteSetting = $null
} -OrgDbOrgSettings -Confirm:$false -Verbose
```

**Verbose Output Example**:
```
VERBOSE: Setting 'ObsoleteSetting': Removing (was 'oldvalue')
VERBOSE: Updated 1 attribute(s) in organization record
```

## Change Tracking and Optimization

The Set cmdlet implements intelligent change tracking:

1. **Retrieves existing record** - Gets current values with all columns
2. **Compares values** - Compares each property with existing value
3. **Updates only changed values** - Only includes changed attributes in the update request
4. **Returns early if no changes** - If nothing changed, no update is performed

This approach:
- Minimizes unnecessary updates
- Provides clear verbose output
- Reduces risk of unintended changes
- Improves performance

## Safety Features

### Confirmation Prompts

The Set cmdlet has `ConfirmImpact.High` and will prompt for confirmation by default:

```powershell
# This will prompt for confirmation
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    maximumtrackingnumber = 100000
}

# Suppress confirmation
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    maximumtrackingnumber = 100000
} -Confirm:$false
```

### WhatIf Support

Preview changes without making them:

```powershell
# See what would be changed without making changes
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    MaxUploadFileSize = 10485760
} -OrgDbOrgSettings -WhatIf
```

**Output**:
```
What if: Performing the operation "Update organization settings" on target "OrgDbOrgSettings in organization record ..."
```

### PassThru Support

Return the updated record after modification:

```powershell
# Get the updated record back
$result = Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    maximumtrackingnumber = 100000
} -PassThru -Confirm:$false

# Use the returned record
Write-Host "Updated organization: $($result.name)"
```

## Common Use Cases

### Adjust File Upload Limits

```powershell
# Increase maximum upload file size to 10 MB
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    MaxUploadFileSize = 10485760
} -OrgDbOrgSettings -Confirm:$false

# Verify the change
$settings = Get-DataverseOrganizationSettings -Connection $conn -OrgDbOrgSettings
$settings.MaxUploadFileSize  # Should show 10485760
```

### Enable/Disable Features

```powershell
# Enable Bing Maps integration
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    EnableBingMapsIntegration = $true
} -OrgDbOrgSettings -Confirm:$false

# Disable saving appointments as draft
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    AllowSaveAsDraftAppointment = $false
} -OrgDbOrgSettings -Confirm:$false
```

### Update Tracking Numbers

```powershell
# Update maximum tracking number
Set-DataverseOrganizationSettings -Connection $conn -InputObject @{
    maximumtrackingnumber = 999999
} -Confirm:$false
```

### Audit Organization Settings

```powershell
# Get current settings
$settings = Get-DataverseOrganizationSettings -Connection $conn -OrgDbOrgSettings

# Export to JSON for audit/backup
$settings | ConvertTo-Json | Out-File "org-settings-backup.json"

# Compare with saved settings
$backup = Get-Content "org-settings-backup.json" | ConvertFrom-Json
Compare-Object -ReferenceObject $backup.PSObject.Properties -DifferenceObject $settings.PSObject.Properties -Property Name, Value
```

## Important Notes

### Separate the Two Worlds

- **Without `-OrgDbOrgSettings`**: Only work with organization table columns
- **With `-OrgDbOrgSettings`**: Only work with OrgDbOrgSettings XML settings
- Never mix the two in the same operation

### Setting Names Are Case-Sensitive

OrgDbOrgSettings XML element names are case-sensitive:

```powershell
# Correct
MaxUploadFileSize = 10485760

# May not work as expected
maxuploadfilesize = 10485760
```

### Type Conversion

When getting OrgDbOrgSettings, values are automatically typed:
- Boolean strings (`"true"`, `"false"`) → `$true`, `$false`
- Numeric strings → Integer values
- Everything else → String values

When setting OrgDbOrgSettings, values are converted to strings in the XML.

### Environment-Wide Impact

Organization settings affect the entire Dataverse environment. Test changes in a non-production environment first.

## See Also

- [Get-DataverseOrganizationSettings](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseOrganizationSettings.md) - Full cmdlet reference
- [Set-DataverseOrganizationSettings](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseOrganizationSettings.md) - Full cmdlet reference
- [Microsoft Documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/organization) - Organization entity reference
- [OrgDbOrgSettings Reference](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/organization#BKMK_OrgDbOrgSettings) - OrgDbOrgSettings column reference
