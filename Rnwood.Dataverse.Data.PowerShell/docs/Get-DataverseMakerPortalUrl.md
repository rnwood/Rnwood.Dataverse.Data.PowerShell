---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseMakerPortalUrl

## SYNOPSIS
Generates a URL to open the Power Apps Maker Portal for the current environment.

## SYNTAX

```
Get-DataverseMakerPortalUrl [-Page <String>] [-Connection <ServiceClient>] 
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet generates a URL that opens the Power Apps Maker Portal for the Dataverse environment associated with the current connection. 

The Maker Portal is where you can:
- Build and configure Power Apps
- Manage solutions
- View and manage tables (entities)
- Create and manage flows
- Work with connections and data sources

You can optionally specify which page of the Maker Portal to navigate to directly.

## EXAMPLES

### Example 1: Get URL for Maker Portal home page
```powershell
PS C:\> Get-DataverseMakerPortalUrl -Connection $c
```

Returns a URL to open the Maker Portal home page for the connected environment.

### Example 2: Get URL for solutions page
```powershell
PS C:\> Get-DataverseMakerPortalUrl -Connection $c -Page "solutions"
```

Returns a URL to open the Solutions page in the Maker Portal.

### Example 3: Get URL for tables page
```powershell
PS C:\> Get-DataverseMakerPortalUrl -Connection $c -Page "tables"
```

Returns a URL to open the Tables page in the Maker Portal.

### Example 4: Open Maker Portal directly in browser
```powershell
PS C:\> Start-Process (Get-DataverseMakerPortalUrl -Connection $c -Page "apps")
```

Opens the Apps page of the Maker Portal directly in the default web browser.

## PARAMETERS

### -Page
Specific page to navigate to in the maker portal. Valid values are: home, solutions, tables, apps, flows, chatbots, connections, dataflows, entities.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: home
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

The Maker Portal URL is always in the format: https://make.powerapps.com/environments/{environmentId}/{page}

## RELATED LINKS
[Power Apps Maker Portal](https://make.powerapps.com)
