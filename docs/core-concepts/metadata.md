# Working with Metadata

Metadata in Dataverse defines the structure of your data - entities (tables), attributes (columns), relationships, and option sets (choice fields). The module provides comprehensive cmdlets for reading and managing metadata programmatically.

## Key Concepts

### What is Metadata?

- **Entity Metadata**: Defines a table (entity) including its display name, primary attribute, ownership model, and behavior
- **Attribute Metadata**: Defines a column (attribute) including its data type, format, constraints, and display properties
- **Relationship Metadata**: Defines connections between entities (1:N, N:1, N:N relationships)
- **Option Set Metadata**: Defines the choices available in choice fields (picklists)

### Why Work with Metadata Programmatically?

- **Automation**: Create and configure solutions automatically
- **Documentation**: Generate documentation from your Dataverse schema
- **Analysis**: Analyze schema complexity and dependencies
- **Migration**: Clone schema between environments
- **Validation**: Ensure schema consistency across environments

## Reading Metadata

All Get cmdlets return raw SDK metadata objects, providing full access to all properties documented by Microsoft's SDK.

### Entity Metadata

```powershell
# Get basic entity information
$contact = Get-DataverseEntityMetadata -EntityName contact

# Access properties
$displayName = $contact.DisplayName.UserLocalizedLabel.Label
$primaryKey = $contact.PrimaryIdAttribute

# Get entity with attributes included
$contactFull = Get-DataverseEntityMetadata -EntityName contact -IncludeAttributes

# List all custom entities
Get-DataverseEntityMetadata | Where-Object { $_.IsCustomEntity -eq $true }
```

### Attribute Metadata

```powershell
# Get single attribute metadata
$attr = Get-DataverseAttributeMetadata -EntityName contact -AttributeName emailaddress1

# Access type-specific properties
if ($attr -is [Microsoft.Xrm.Sdk.Metadata.StringAttributeMetadata]) {
    $maxLength = $attr.MaxLength
    $format = $attr.Format
}

# Get all attributes for an entity
$attributes = Get-DataverseAttributeMetadata -EntityName contact

# Find all required attributes
$required = $attributes | Where-Object { $_.RequiredLevel.Value -eq 'ApplicationRequired' }
```

### Relationship Metadata

```powershell
# Get all relationships for an entity
$relationships = Get-DataverseRelationshipMetadata -EntityName contact

# Filter by type
$oneToMany = Get-DataverseRelationshipMetadata -EntityName contact -RelationshipType OneToMany
$manyToMany = Get-DataverseRelationshipMetadata -EntityName contact -RelationshipType ManyToMany

# Get specific relationship
$rel = Get-DataverseRelationshipMetadata -SchemaName contact_customer_accounts
```

### Option Set Metadata

```powershell
# Get option set for an attribute
$options = Get-DataverseOptionSetMetadata -EntityName contact -AttributeName preferredcontactmethodcode

# Access option values
foreach ($option in $options.Options) {
    Write-Host "$($option.Value): $($option.Label.UserLocalizedLabel.Label)"
}

# List all global option sets
$globalOptionSets = Get-DataverseOptionSetMetadata
```

## Metadata Caching

The module includes intelligent metadata caching to improve performance when working with metadata operations.

### How Caching Works

- **Opt-in per call**: Add `-UseMetadataCache` parameter to Get cmdlets
- **Automatic invalidation**: Set and Remove operations automatically clear affected cache entries
- **Filter-aware**: Different filter combinations (e.g., with/without attributes) are cached separately
- **Connection-specific**: Cache is isolated per connection

### Using the Cache

```powershell
# First call fetches from server and caches
$entity1 = Get-DataverseEntityMetadata -EntityName contact -IncludeAttributes -UseMetadataCache

# Second call returns from cache (fast)
$entity2 = Get-DataverseEntityMetadata -EntityName contact -IncludeAttributes -UseMetadataCache

# Different filters fetch separately
$entityBasic = Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache  # Not from cache
```

### Manual Cache Management

```powershell
# Clear all cached metadata
Clear-DataverseMetadataCache

# Clear for specific connection
Clear-DataverseMetadataCache -Connection $myConnection
```

### When to Use Caching

**Use caching when:**
- Making multiple metadata queries in a script
- Metadata is unlikely to change during script execution
- Performance is critical

**Don't use caching when:**
- Metadata might be changing (e.g., during migrations)
- You need guaranteed up-to-date information
- Working interactively and schema might be modified externally

## Creating and Updating Metadata

The Set cmdlets handle both creating new metadata and updating existing metadata.

### Creating Entities

```powershell
# Create a new custom entity
Set-DataverseEntityMetadata `
    -EntityName new_project `
    -SchemaName new_Project `
    -DisplayName "Project" `
    -PrimaryAttributeSchemaName new_name `
    -OwnershipType UserOwned `
    -HasActivities
```

### Creating Attributes

Different attribute types require different parameters. See [Metadata CRUD Examples](../Metadata-CRUD-Examples.md) for comprehensive examples of all attribute types.

```powershell
# String attribute with format
Set-DataverseAttributeMetadata `
    -EntityName new_project `
    -AttributeName new_email `
    -SchemaName new_Email `
    -AttributeType String `
    -StringFormat Email `
    -MaxLength 100 `
    -DisplayName "Email Address"

# Decimal with precision
Set-DataverseAttributeMetadata `
    -EntityName new_project `
    -AttributeName new_budget `
    -SchemaName new_Budget `
    -AttributeType Decimal `
    -Precision 2 `
    -MinValue 0 `
    -MaxValue 9999999.99 `
    -DisplayName "Budget"

# Choice field (Picklist)
Set-DataverseAttributeMetadata `
    -EntityName new_project `
    -AttributeName new_status `
    -SchemaName new_Status `
    -AttributeType Picklist `
    -Options @(
        @{Value=1; Label='Planning'}
        @{Value=2; Label='Active'}
        @{Value=3; Label='Completed'}
    )
```

### Creating Relationships

```powershell
# Create OneToMany relationship (adds lookup attribute)
Set-DataverseRelationshipMetadata `
    -SchemaName new_project_contact `
    -RelationshipType OneToMany `
    -ReferencedEntity new_project `
    -ReferencingEntity contact `
    -LookupAttributeSchemaName new_ProjectId `
    -LookupAttributeDisplayName "Project" `
    -CascadeDelete RemoveLink

# Create ManyToMany relationship (creates intersect entity)
Set-DataverseRelationshipMetadata `
    -SchemaName new_project_contact `
    -RelationshipType ManyToMany `
    -Entity1 new_project `
    -Entity2 contact
```

### Creating Global Option Sets

```powershell
# Create a reusable option set
Set-DataverseOptionSetMetadata `
    -Name new_priority `
    -DisplayName "Priority" `
    -Options @(
        @{Value=1; Label='Low'; Color='#00FF00'}
        @{Value=2; Label='Medium'; Color='#FFFF00'}
        @{Value=3; Label='High'; Color='#FF0000'}
    )
```

## Deleting Metadata

Remove cmdlets permanently delete metadata. Use with caution.

### Safety Features

- **Confirmation prompts**: Required by default for destructive operations
- **-WhatIf support**: Preview what would be deleted
- **-Force parameter**: Bypass confirmation when automating

```powershell
# Delete with confirmation prompt
Remove-DataverseAttributeMetadata -EntityName new_project -AttributeName new_oldfield

# Skip confirmation (use in automation)
Remove-DataverseAttributeMetadata -EntityName new_project -AttributeName new_oldfield -Force

# Preview without deleting
Remove-DataverseAttributeMetadata -EntityName new_project -AttributeName new_oldfield -WhatIf
```

## Common Patterns

### Schema Documentation

```powershell
# Document all custom entities and their attributes
$customEntities = Get-DataverseEntityMetadata -UseMetadataCache | 
    Where-Object { $_.IsCustomEntity }

foreach ($entity in $customEntities) {
    $fullEntity = Get-DataverseEntityMetadata -EntityName $entity.LogicalName -IncludeAttributes -UseMetadataCache
    
    Write-Host "## $($fullEntity.DisplayName.UserLocalizedLabel.Label)"
    
    $customAttrs = $fullEntity.Attributes | Where-Object { $_.IsCustomAttribute }
    foreach ($attr in $customAttrs) {
        Write-Host "  - $($attr.LogicalName): $($attr.AttributeType)"
    }
}
```

### Schema Validation

```powershell
# Check for required fields without defaults
$entity = Get-DataverseEntityMetadata -EntityName contact -IncludeAttributes

$problematicFields = $entity.Attributes | Where-Object {
    $_.RequiredLevel.Value -eq 'ApplicationRequired' -and
    $_.DefaultValue -eq $null
}

if ($problematicFields) {
    Write-Warning "Found required fields without defaults:"
    $problematicFields | ForEach-Object { Write-Host "  - $($_.LogicalName)" }
}
```

### Bulk Attribute Creation

```powershell
# Create multiple attributes from a definition
$attributeDefinitions = @(
    @{Name='new_name'; Type='String'; MaxLength=100; DisplayName='Name'}
    @{Name='new_description'; Type='Memo'; MaxLength=2000; DisplayName='Description'}
    @{Name='new_startdate'; Type='DateTime'; DisplayName='Start Date'}
)

foreach ($def in $attributeDefinitions) {
    Set-DataverseAttributeMetadata `
        -EntityName new_project `
        -AttributeName $def.Name `
        -AttributeType $def.Type `
        -DisplayName $def.DisplayName `
        @(if ($def.MaxLength) { @{MaxLength=$def.MaxLength} } else { @{} })
}
```

## Best Practices

1. **Use caching for read-heavy operations**: Significantly improves performance
2. **Always specify SchemaName**: Required for new metadata, use proper naming conventions
3. **Test with -WhatIf first**: Preview destructive operations before executing
4. **Handle SDK objects directly**: Access properties using SDK documentation (e.g., `$entity.DisplayName.UserLocalizedLabel.Label`)
5. **Check for existence before creating**: Avoid errors by checking if metadata already exists
6. **Use proper cascade settings**: Understand relationship cascade behavior before creating relationships
7. **Document your schema changes**: Keep track of metadata modifications in your automation scripts

## Related Resources

- [Metadata CRUD Examples](../Metadata-CRUD-Examples.md) - Comprehensive examples for all attribute types and scenarios
- [Microsoft SDK Documentation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.metadata) - SDK metadata object reference
- [Dataverse Metadata Reference](https://docs.microsoft.com/en-us/power-apps/developer/data-platform/metadata-services) - Overview of metadata concepts
