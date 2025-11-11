---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseAdminPortalUrl

## SYNOPSIS
Generates a URL to open the Power Platform Admin Center for the current environment.

## SYNTAX

```
Get-DataverseAdminPortalUrl [-Page <String>] [-Connection <ServiceClient>] 
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet generates a URL that opens the Power Platform Admin Center for the Dataverse environment associated with the current connection.

The Admin Center is where administrators can:
- Manage environments
- View analytics and reports
- Configure data integration
- Manage resources and capacity
- Set up data policies
- Access help and support

You can optionally specify which section of the Admin Center to navigate to directly.

## EXAMPLES

### Example 1: Get URL for specific environment in Admin Center
```powershell
PS C:\> Get-DataverseAdminPortalUrl -Connection $c
```

Returns a URL to open the Admin Center for the connected environment (defaults to environments page).

### Example 2: Get URL for analytics page
```powershell
PS C:\> Get-DataverseAdminPortalUrl -Connection $c -Page "analytics"
```

Returns a URL to open the Analytics page in the Admin Center.

### Example 3: Get URL for data policies page
```powershell
PS C:\> Get-DataverseAdminPortalUrl -Connection $c -Page "datapolicies"
```

Returns a URL to open the Data Policies page in the Admin Center.

### Example 4: Open Admin Center directly in browser
```powershell
PS C:\> Start-Process (Get-DataverseAdminPortalUrl -Connection $c -Page "resources")
```

Opens the Resources page of the Admin Center directly in the default web browser.

## PARAMETERS

### -Page
Specific page to navigate to in the admin portal. Valid values are: home, environments, analytics, resources, dataintegration, datapolicies, helpandsupport.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: environments
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/). If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.String

## NOTES

The Admin Center URL format varies by page:
- For environments: https://admin.powerplatform.microsoft.com/environments/{environmentId}/hub
- For other pages: https://admin.powerplatform.microsoft.com/{page}

Administrative permissions may be required to access certain sections of the Admin Center.

## RELATED LINKS
[Power Platform Admin Center](https://admin.powerplatform.microsoft.com)
