---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseSitemapEntry

## SYNOPSIS
Creates or updates an entry (Area, Group, or SubArea) in a Dataverse sitemap.

## SYNTAX

### Area
```
Set-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [-SitemapUniqueName <String>] [-SitemapId <Guid>] [-Area] -EntryId <String> [-ResourceId <String>]
 [-DescriptionResourceId <String>] [-ToolTipResourceId <String>] [-Title <String>] [-Description <String>]
 [-Icon <String>] [-Entity <String>] [-Url <String>] [-Index <Int32>] [-Before <String>] [-After <String>]
 [-IsDefault] [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

### Group
```
Set-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [-SitemapUniqueName <String>] [-SitemapId <Guid>] [-Group] -EntryId <String> [-ResourceId <String>]
 [-DescriptionResourceId <String>] [-ToolTipResourceId <String>] [-Title <String>] [-Description <String>]
 [-Icon <String>] [-Entity <String>] [-Url <String>] [-ParentAreaId <String>] [-Index <Int32>]
 [-Before <String>] [-After <String>] [-IsDefault] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### SubArea
```
Set-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [-SitemapUniqueName <String>] [-SitemapId <Guid>] [-SubArea] -EntryId <String> [-ResourceId <String>]
 [-DescriptionResourceId <String>] [-ToolTipResourceId <String>] [-Title <String>] [-Description <String>]
 [-Icon <String>] [-Entity <String>] [-Url <String>] [-ParentAreaId <String>] [-ParentGroupId <String>]
 [-Index <Int32>] [-Before <String>] [-After <String>] [-IsDefault] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Privilege
```
Set-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [-SitemapUniqueName <String>] [-SitemapId <Guid>] [-Privilege] [-EntryId <String>] [-ResourceId <String>]
 [-DescriptionResourceId <String>] [-ToolTipResourceId <String>] [-Title <String>] [-Description <String>]
 [-Icon <String>] [-Entity <String>] [-Url <String>] -ParentSubAreaId <String> -PrivilegeEntity <String>
 -PrivilegeName <String> [-Index <Int32>] [-Before <String>] [-After <String>] [-IsDefault] [-PassThru]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseSitemapEntry cmdlet creates or updates navigation entries (Areas, Groups, SubAreas) in a Dataverse sitemap. It automatically detects whether an entry exists and either creates a new entry or updates the existing one. The cmdlet supports precise positioning control when creating entries.

## EXAMPLES

### Example 1: Create a new Area
```powershell
PS C:\> Set-DataverseSitemapEntry -SitemapName "MySitemap" -EntryType Area -EntryId "SalesArea" -Title "Sales" -Icon "/_imgs/area/sales_24x24.gif"
```

Creates a new Area entry in the sitemap.

### Example 2: Create a Group with position control
```powershell
PS C:\> Set-DataverseSitemapEntry -SitemapName "MySitemap" -EntryType Group -EntryId "LeadsGroup" -ParentAreaId "SalesArea" -Title "Leads" -Index 0
```

Creates a new Group at the first position (index 0) within the SalesArea.

### Example 3: Create a SubArea before another entry
```powershell
PS C:\> Set-DataverseSitemapEntry -SitemapName "MySitemap" -EntryType SubArea -EntryId "AccountsSubarea" `
    -ParentAreaId "SalesArea" -ParentGroupId "CustomersGroup" `
    -Entity "account" -Title "Accounts" -Before "ContactsSubarea"
```

Creates a new SubArea positioned before the "ContactsSubarea" entry.

### Example 4: Update an existing entry
```powershell
PS C:\> Set-DataverseSitemapEntry -SitemapName "MySitemap" -EntryType Area -EntryId "SalesArea" -Title "Sales & Marketing"
```

Updates the title of an existing Area entry.

### Example 5: Use pipeline to update entry
```powershell
PS C:\> Get-DataverseSitemapEntry -SitemapName "MySitemap" -EntryId "SalesArea" | Set-DataverseSitemapEntry -Title "Updated Sales"
```

Retrieves an entry and updates its title using the pipeline.

## PARAMETERS

### -After
The ID of the sibling entry after which this entry should be inserted or moved.

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

### -Before
The ID of the sibling entry before which this entry should be inserted or moved.

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

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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

### -Description
The new description of the entry.

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

### -Entity
The new entity logical name (for SubAreas).

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

### -EntryId
The ID of the entry to update.

```yaml
Type: String
Parameter Sets: Area, Group, SubArea
Aliases: Id

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Privilege
Aliases: Id

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Icon
The new icon path (for Areas and SubAreas).

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

### -Index
The zero-based index position where the entry should be inserted or moved to.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Entry object from Get-DataverseSitemapEntry.

```yaml
Type: SitemapEntryInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -IsDefault
Whether the entry is a default entry.

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

### -ParentAreaId
The parent Area ID (for locating Groups and SubAreas).

```yaml
Type: String
Parameter Sets: Group, SubArea
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ParentGroupId
The parent Group ID (for locating SubAreas).

```yaml
Type: String
Parameter Sets: SubArea
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If specified, returns the created or updated entry.

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

### -Privilege
The privilege required to view this entry.

```yaml
Type: SwitchParameter
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ResourceId
The new resource ID for localized titles.

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
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SitemapId
The ID of the sitemap containing the entry.

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

### -Title
The new title/label of the entry.

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

### -Url
The new URL (for SubAreas).

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

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Determines how PowerShell responds to progress updates generated by the cmdlet.

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

### -SitemapUniqueName
The unique name of the sitemap containing the entry.

```yaml
Type: String
Parameter Sets: (All)
Aliases: Name

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Area
Create or update an Area entry.

```yaml
Type: SwitchParameter
Parameter Sets: Area
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DescriptionResourceId
The resource ID for localized descriptions.

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

### -Group
Create or update a Group entry.

```yaml
Type: SwitchParameter
Parameter Sets: Group
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SubArea
Create or update a SubArea entry.

```yaml
Type: SwitchParameter
Parameter Sets: SubArea
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ToolTipResourceId
The resource ID for localized tooltips.

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

### -ParentSubAreaId
The parent SubArea ID (required for Privileges when creating).

```yaml
Type: String
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrivilegeEntity
The entity name for privilege entries.

```yaml
Type: String
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrivilegeName
The privilege name for privilege entries (e.g., Read, Write, Create, Delete).

```yaml
Type: String
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryInfo
### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapInfo
### System.String
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=9.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryInfo
## NOTES

## RELATED LINKS
