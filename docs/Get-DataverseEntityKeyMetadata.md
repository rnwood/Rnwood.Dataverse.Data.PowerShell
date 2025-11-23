---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseEntityKeyMetadata

## SYNOPSIS
Retrieves alternate key metadata for an entity (table) from Dataverse.

## SYNTAX

```
Get-DataverseEntityKeyMetadata [-Connection <IOrganizationService>] -EntityName <String> [-KeyName <String>]
 [-UseMetadataCache] [-Published] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseEntityKeyMetadata cmdlet retrieves alternate key metadata for a specific entity in Dataverse.
You can retrieve a specific key by name or all keys for the entity.
Alternate keys provide a way to uniquely identify records using combinations of attributes other than the primary key.

## EXAMPLES

### Example 1: Get all alternate keys for an entity
```powershell
PS C:\> $connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact
```

This command retrieves all alternate keys defined on the contact entity.

### Example 2: Get a specific alternate key
```powershell
PS C:\> Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -KeyName "contact_emailaddress1_key"
```

This command retrieves the specific alternate key named "contact_emailaddress1_key" from the contact entity.

### Example 3: Get keys using the metadata cache
```powershell
PS C:\> Get-DataverseEntityKeyMetadata -Connection $connection -EntityName account -UseMetadataCache
```

This command retrieves alternate keys for the account entity using the shared metadata cache for improved performance.

### Example 4: Get only published keys
```powershell
PS C:\> Get-DataverseEntityKeyMetadata -Connection $connection -EntityName contact -Published
```

This command retrieves only published alternate keys, excluding any unpublished draft changes.

## PARAMETERS

### -Connection
The Dataverse connection to use. If not specified, uses the default connection.

```yaml
Type: IOrganizationService
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EntityName
The logical name of the entity (table) to retrieve key metadata for.

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -KeyName
The logical name of the specific key to retrieve. If not specified, returns all keys for the entity.

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

### -UseMetadataCache
If specified, uses the shared global metadata cache for improved performance.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Published
If specified, retrieves only published metadata. By default, unpublished (draft) metadata is retrieved which includes all changes.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
You can pipe an entity name to this cmdlet.

## OUTPUTS

### Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata
This cmdlet returns EntityKeyMetadata objects containing information about alternate keys.

## NOTES
- Alternate keys are used to uniquely identify records using combinations of attributes
- Keys are typically used for data integration scenarios where external systems need to reference records
- Use the UseMetadataCache parameter when making repeated calls for better performance

## RELATED LINKS
[Set-DataverseEntityKeyMetadata](Set-DataverseEntityKeyMetadata.md)
[Remove-DataverseEntityKeyMetadata](Remove-DataverseEntityKeyMetadata.md)
[Define alternate keys for an entity](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/define-alternate-keys-entity)
