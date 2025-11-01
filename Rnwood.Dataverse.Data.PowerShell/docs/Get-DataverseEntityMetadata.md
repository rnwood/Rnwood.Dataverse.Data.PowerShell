---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseEntityMetadata

## SYNOPSIS
Retrieves entity (table) metadata from Dataverse.

## SYNTAX

```
Get-DataverseEntityMetadata [[-EntityName] <String>] [-IncludeAttributes] [-IncludeRelationships]
 [-IncludePrivileges] [-UseMetadataCache] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The `Get-DataverseEntityMetadata` cmdlet retrieves metadata information about entities (tables) in Dataverse. You can retrieve metadata for a specific entity or all entities in the organization.

The cmdlet returns comprehensive entity information including:
- Logical name and schema name
- Display names (singular and plural)
- Primary key and primary name attributes
- Ownership type (User, Team, Organization)
- Entity capabilities (activities, notes, audit, change tracking)
- Custom vs system entity status
- Optionally: attributes, relationships, and privileges

Metadata is essential for:
- Understanding entity structure and capabilities
- Building dynamic forms and queries
- Validating entity existence and properties
- Schema documentation and analysis
- Migration and deployment planning

**Performance Tip:** Use `-UseMetadataCache` to enable caching for improved performance when repeatedly accessing metadata.

## EXAMPLES

### Example 1: Get metadata for a specific entity
```powershell
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName contact
PS C:\> $metadata

LogicalName          : contact
SchemaName           : Contact
DisplayName          : Contact
DisplayCollectionName: Contacts
PrimaryIdAttribute   : contactid
PrimaryNameAttribute : fullname
OwnershipType        : UserOwned
IsCustomEntity       : False
```

Retrieves basic metadata for the `contact` entity.

### Example 2: Get metadata with attributes included
```powershell
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account -IncludeAttributes
PS C:\> $metadata.Attributes.Count
150

PS C:\> $metadata.Attributes | Select-Object -First 5 LogicalName, AttributeType

LogicalName     AttributeType
-----------     -------------
accountid       Uniqueidentifier
accountname     String
accountnumber   String
address1_city   String
address1_country String
```

Retrieves entity metadata including all attributes.

### Example 3: Get metadata with relationships
```powershell
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account -IncludeRelationships
PS C:\> $metadata.OneToManyRelationships.Count
45

PS C:\> $metadata.OneToManyRelationships | Select-Object -First 3 SchemaName, ReferencingEntity

SchemaName                  ReferencingEntity
----------                  -----------------
account_primary_contact     contact
account_customer_accounts   account
account_parent_account      account
```

Retrieves entity metadata including all relationships.

### Example 4: Get comprehensive metadata (all options)
```powershell
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName contact `
    -IncludeAttributes -IncludeRelationships -IncludePrivileges

PS C:\> [PSCustomObject]@{
    Entity = $metadata.LogicalName
    Attributes = $metadata.Attributes.Count
    OneToMany = $metadata.OneToManyRelationships.Count
    ManyToOne = $metadata.ManyToOneRelationships.Count
    ManyToMany = $metadata.ManyToManyRelationships.Count
    Privileges = $metadata.Privileges.Count
}

Entity     : contact
Attributes : 120
OneToMany  : 38
ManyToOne  : 15
ManyToMany : 5
Privileges : 8
```

Retrieves complete entity metadata including attributes, relationships, and privileges.

### Example 5: List all entities in the organization
```powershell
PS C:\> $allEntities = Get-DataverseEntityMetadata
PS C:\> $allEntities.Count
450

PS C:\> $allEntities | Select-Object -First 10 LogicalName, DisplayName, IsCustomEntity

LogicalName          DisplayName          IsCustomEntity
-----------          -----------          --------------
account              Account              False
contact              Contact              False
lead                 Lead                 False
opportunity          Opportunity          False
new_project          Project              True
new_task             Task                 True
```

Retrieves basic metadata for all entities in the organization.

### Example 6: Filter to custom entities only
```powershell
PS C:\> $customEntities = Get-DataverseEntityMetadata | Where-Object { $_.IsCustomEntity -eq $true }
PS C:\> $customEntities | Select-Object LogicalName, DisplayName, SchemaName

LogicalName     DisplayName     SchemaName
-----------     -----------     ----------
new_project     Project         new_Project
new_task        Task            new_Task
new_resource    Resource        new_Resource
```

Retrieves only custom (user-created) entities.

### Example 7: Find entities with audit enabled
```powershell
PS C:\> $auditEntities = Get-DataverseEntityMetadata | 
    Where-Object { $_.IsAuditEnabled.Value -eq $true }

PS C:\> $auditEntities | Select-Object LogicalName, DisplayName

LogicalName     DisplayName
-----------     -----------
account         Account
contact         Contact
opportunity     Opportunity
```

Finds all entities with auditing enabled.

### Example 8: Find entities that support activities
```powershell
PS C:\> $activityEntities = Get-DataverseEntityMetadata | 
    Where-Object { $_.IsActivityParty.Value -eq $true }

PS C:\> $activityEntities.Count
25
```

Finds all entities that can be associated with activities.

### Example 9: Export entity list to CSV
```powershell
PS C:\> Get-DataverseEntityMetadata | 
    Select-Object LogicalName, DisplayName, IsCustomEntity, OwnershipType, IsAuditEnabled | 
    Export-Csv -Path "entities.csv" -NoTypeInformation
```

Exports a list of all entities with key properties to CSV for documentation.

### Example 10: Use metadata cache for performance
```powershell
PS C:\> # First call - fetches from server
PS C:\> Measure-Command { $metadata1 = Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache }

Milliseconds : 450

PS C:\> # Second call - uses cache
PS C:\> Measure-Command { $metadata2 = Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache }

Milliseconds : 2
```

Demonstrates the performance improvement when using the metadata cache.

### Example 11: Compare entity metadata between environments
```powershell
PS C:\> $dev = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive
PS C:\> $prod = Get-DataverseConnection -Url "https://prod.crm.dynamics.com" -Interactive

PS C:\> $devEntities = Get-DataverseEntityMetadata -Connection $dev | Select-Object -ExpandProperty LogicalName
PS C:\> $prodEntities = Get-DataverseEntityMetadata -Connection $prod | Select-Object -ExpandProperty LogicalName

PS C:\> $onlyInDev = $devEntities | Where-Object { $_ -notin $prodEntities }
PS C:\> $onlyInProd = $prodEntities | Where-Object { $_ -notin $devEntities }

PS C:\> Write-Host "Entities only in Dev: $($onlyInDev -join ', ')"
PS C:\> Write-Host "Entities only in Prod: $($onlyInProd -join ', ')"
```

Compares entities between development and production environments.

### Example 12: Find entities by display name pattern
```powershell
PS C:\> Get-DataverseEntityMetadata | 
    Where-Object { $_.DisplayName.UserLocalizedLabel.Label -like "*Project*" } |
    Select-Object LogicalName, DisplayName

LogicalName     DisplayName
-----------     -----------
new_project     Project
new_projecttask Project Task
```

Finds entities with "Project" in their display name.

### Example 13: Pipeline entity name to get metadata
```powershell
PS C:\> @("account", "contact", "lead") | Get-DataverseEntityMetadata | 
    Select-Object LogicalName, DisplayName, PrimaryNameAttribute

LogicalName DisplayName PrimaryNameAttribute
----------- ----------- --------------------
account     Account     name
contact     Contact     fullname
lead        Lead        fullname
```

Pipelines multiple entity names to retrieve their metadata.

### Example 14: Get entities with change tracking enabled
```powershell
PS C:\> Get-DataverseEntityMetadata | 
    Where-Object { $_.ChangeTrackingEnabled -eq $true } |
    Select-Object LogicalName, DisplayName

LogicalName     DisplayName
-----------     -----------
account         Account
contact         Contact
```

Finds entities with change tracking enabled for data synchronization.

### Example 15: Work with default connection
```powershell
PS C:\> # Set a default connection
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Set-DataverseConnectionAsDefault -Connection $conn

PS C:\> # Now get metadata without specifying connection
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account
PS C:\> $metadata.LogicalName
account
```

Demonstrates using the default connection for simplified commands.

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

### -EntityName
Logical name of the entity to retrieve metadata for.
If not specified, returns all entities.

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -IncludeAttributes
Include attribute (column) metadata in the output

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

### -IncludePrivileges
Include privilege metadata in the output

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

### -IncludeRelationships
Include relationship metadata in the output

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

### -UseMetadataCache
Use the shared global metadata cache for improved performance

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

### System.String
## OUTPUTS

### Microsoft.Xrm.Sdk.Metadata.EntityMetadata
## NOTES

## RELATED LINKS
