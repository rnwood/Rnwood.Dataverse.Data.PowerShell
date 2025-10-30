---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSitemap

## SYNOPSIS
Retrieves sitemap information from a Dataverse environment.

## SYNTAX

```
Get-DataverseSitemap [[-Name] <String>] [-Id <Guid>] [-SolutionUniqueName <String>] [-AppUniqueName <String>]
 [-Managed] [-Unmanaged] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves sitemap information from a Dataverse environment. Sitemaps define the navigation structure for model-driven apps.

You can retrieve all sitemaps or filter by name, ID, solution, app, or managed status.

The cmdlet returns SitemapInfo objects with metadata about each sitemap including name, XML definition, solution association, and more.

## EXAMPLES

### Example 1: Retrieve all sitemaps
```powershell
PS C:\> Get-DataverseSitemap

Name              Id                                   IsManaged SolutionName
----              --                                   --------- ------------
MySitemap         a1b2c3d4-5678-90ab-cdef-1234567890ab False     MySolution
DefaultSitemap    f1e2d3c4-b5a6-9087-6543-210fedcba987 True      Default
```

Retrieves all sitemaps from the Dataverse environment.

### Example 2: Retrieve a specific sitemap by name
```powershell
PS C:\> Get-DataverseSitemap -Name "MySitemap"

Name              : MySitemap
Id                : a1b2c3d4-5678-90ab-cdef-1234567890ab
IsManaged         : False
SolutionName      : MySolution
AppUniqueName     : myapp
CreatedOn         : 1/15/2024 10:30:00 AM
ModifiedOn        : 1/20/2024 2:45:00 PM
SitemapXml        : <SiteMap>...</SiteMap>
```

Retrieves a specific sitemap by its name with full details.

### Example 3: Get sitemaps for a specific solution
```powershell
PS C:\> Get-DataverseSitemap -SolutionUniqueName "MySolution"

Name              Id                                   IsManaged
----              --                                   ---------
MySitemap         a1b2c3d4-5678-90ab-cdef-1234567890ab False
AnotherSitemap    b2c3d4e5-6789-01bc-def2-345678901bcd False
```

Retrieves all sitemaps belonging to a specific solution.

### Example 4: Get sitemap for a specific app
```powershell
PS C:\> Get-DataverseSitemap -AppUniqueName "myapp"

Name              : MySitemap
Id                : a1b2c3d4-5678-90ab-cdef-1234567890ab
AppUniqueName     : myapp
SitemapXml        : <SiteMap>...</SiteMap>
```

Retrieves the sitemap associated with a specific app.

### Example 5: Get only unmanaged sitemaps
```powershell
PS C:\> Get-DataverseSitemap -Unmanaged

Name              Id                                   IsManaged
----              --                                   ---------
MySitemap         a1b2c3d4-5678-90ab-cdef-1234567890ab False
CustomSitemap     c3d4e5f6-7890-12cd-ef34-56789012cdef False
```

Retrieves only unmanaged sitemaps that can be edited.

### Example 6: Export sitemap XML to file
```powershell
PS C:\> $sitemap = Get-DataverseSitemap -Name "MySitemap"
PS C:\> $sitemap.SitemapXml | Out-File -FilePath "MySitemap.xml"
```

Retrieves a sitemap and exports its XML definition to a file.

## PARAMETERS

### -AppUniqueName
Filter sitemaps associated with a specific app unique name.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
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

### -Id
The unique identifier of the sitemap to retrieve.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Managed
Filter to return only managed sitemaps.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
The name of the sitemap to retrieve. If not specified, all sitemaps are returned.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SolutionUniqueName
Filter sitemaps by solution unique name.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Unmanaged
Filter to return only unmanaged sitemaps.

```yaml
Type: SwitchParameter
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

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapInfo
## NOTES

This cmdlet requires an active connection to a Dataverse environment.

Sitemaps define the navigation structure for model-driven apps in Dataverse. The sitemap XML follows the SiteMap schema and contains Area, Group, and SubArea elements that define the app navigation.

## RELATED LINKS

[Set-DataverseSitemap](Set-DataverseSitemap.md)

[Remove-DataverseSitemap](Remove-DataverseSitemap.md)

[Get-DataverseRecord](Get-DataverseRecord.md)
