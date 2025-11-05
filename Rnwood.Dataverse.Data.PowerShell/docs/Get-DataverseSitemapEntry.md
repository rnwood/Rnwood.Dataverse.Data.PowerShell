---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSitemapEntry

## SYNOPSIS
Retrieves sitemap entries (Areas, Groups, SubAreas) from a Dataverse sitemap.

## SYNTAX

```
Get-DataverseSitemapEntry [-Sitemap <SitemapInfo>] [[-SitemapName] <String>] [-SitemapId <Guid>]
 [-EntryType <SitemapEntryType>] [-EntryId <String>] [-ParentAreaId <String>] [-ParentGroupId <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseSitemapEntry cmdlet retrieves navigation entries (Areas, Groups, SubAreas) from a Dataverse sitemap XML definition. It supports filtering by entry type, ID, and parent relationships.

## EXAMPLES

### Example 1: Get all entries from a sitemap
```powershell
PS C:\> Get-DataverseSitemapEntry -SitemapName "MySitemap"
```

Retrieves all navigation entries (Areas, Groups, and SubAreas) from the specified sitemap.

### Example 2: Get only Areas
```powershell
PS C:\> Get-DataverseSitemapEntry -SitemapName "MySitemap" -EntryType Area
```

Retrieves only Area entries from the sitemap.

### Example 3: Get a specific entry
```powershell
PS C:\> Get-DataverseSitemapEntry -SitemapName "MySitemap" -EntryId "SalesArea"
```

Retrieves a specific entry by its ID.

### Example 4: Get Groups within an Area
```powershell
PS C:\> Get-DataverseSitemapEntry -SitemapName "MySitemap" -EntryType Group -ParentAreaId "SalesArea"
```

Retrieves all Groups within the specified Area.

### Example 5: Use pipeline
```powershell
PS C:\> Get-DataverseSitemap -Name "MySitemap" | Get-DataverseSitemapEntry -EntryType Area
```

Pipes a sitemap object to retrieve its Area entries.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -EntryId
The ID of a specific entry to retrieve.

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

### -EntryType
The type of entries to retrieve (Area, Group, SubArea).
If not specified, all types are returned.

```yaml
Type: SitemapEntryType
Parameter Sets: (All)
Aliases:
Accepted values: Area, Group, SubArea

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ParentAreaId
Filter entries by parent Area ID (for Groups and SubAreas).

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

### -ParentGroupId
Filter SubAreas by parent Group ID.

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

### -Sitemap
Sitemap object from Get-DataverseSitemap.

```yaml
Type: SitemapInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SitemapId
The ID of the sitemap to retrieve entries from.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SitemapName
The name of the sitemap to retrieve entries from.

```yaml
Type: String
Parameter Sets: (All)
Aliases: Name

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapInfo
### System.String
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryInfo
## NOTES

## RELATED LINKS
