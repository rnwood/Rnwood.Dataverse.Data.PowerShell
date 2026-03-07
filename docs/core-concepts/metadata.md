
# Working with Metadata

## Reading Metadata

All Get cmdlets return raw SDK metadata objects, providing full access to all properties documented by Microsoft's SDK.

### Entity Metadata

```powershell
# Get basic entity information
$contact = Get-DataverseEntityMetadata -EntityName contact

# Access properties
$displayName = $contact.DisplayName.UserLocalizedLabel.Label
$primaryKey = $contact.PrimaryIdAttribute

# Access icon properties
$vectorIcon = $contact.IconVectorName
$largeIcon = $contact.IconLargeName
$mediumIcon = $contact.IconMediumName
$smallIcon = $contact.IconSmallName

# Get entity with attributes included
$contactFull = Get-DataverseEntityMetadata -EntityName contact -IncludeAttributes

# List all custom entities
Get-DataverseEntityMetadata | Where-Object { $_.IsCustomEntity -eq $true }

# View entities with their icon properties
Get-DataverseEntityMetadata |
    Select-Object LogicalName, DisplayName, IconVectorName, IconLargeName |
    Format-Table -AutoSize
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
    -DisplayCollectionName "Projects" `
    -PrimaryAttributeSchemaName new_name `
    -OwnershipType UserOwned `
    -HasActivities
```

**Setting Icon Properties**

Customize entity icons for better visual identification in the UI:

```powershell
# Create entity with custom icons
Set-DataverseEntityMetadata `
    -EntityName new_product `
    -SchemaName new_Product `
    -DisplayName "Product" `
    -DisplayCollectionName "Products" `
    -PrimaryAttributeSchemaName new_name `
    -OwnershipType UserOwned `
    -IconVectorName "svg_product" `
    -IconLargeName "Entity/product_large.png" `
    -IconMediumName "Entity/product_medium.png" `
    -IconSmallName "Entity/product_small.png"

# Update icons on existing entity
Set-DataverseEntityMetadata `
    -EntityName account `
    -IconVectorName "svg_custom_account"
```

**Using EntityMetadata Objects**

For complex updates, retrieve and modify the EntityMetadata object directly:

```powershell
# Get entity metadata
$metadata = Get-DataverseEntityMetadata -EntityName account

# Modify multiple properties
$metadata.IconVectorName = "svg_updated_account"
$metadata.ChangeTrackingEnabled = $true

# Update entity
Set-DataverseEntityMetadata -EntityMetadata $metadata

# Pipeline example - update multiple entities
Get-DataverseEntityMetadata |
    Where-Object { $_.IsCustomEntity } |
    ForEach-Object {
        $_.IconVectorName = "svg_custom_icon"
        $_
    } |
    Set-DataverseEntityMetadata
```

### Creating Attributes

Different attribute types require different parameters. See the docs for comprehensive examples of all attribute types.

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

# Lookup field with automatic relationship creation
Set-DataverseAttributeMetadata `
    -EntityName contact `
    -AttributeName new_accountid `
    -SchemaName new_AccountId `
    -AttributeType Lookup `
    -DisplayName "Account" `
    -Targets @('account') `
    -RequiredLevel None

# Lookup with custom relationship name and cascade behaviors
Set-DataverseAttributeMetadata `
    -EntityName new_task `
    -AttributeName new_projectid `
    -SchemaName new_ProjectId `
    -AttributeType Lookup `
    -DisplayName "Project" `
    -Targets @('new_project') `
    -RelationshipSchemaName new_project_task `
    -CascadeDelete Cascade `
    -CascadeAssign Cascade `
    -RequiredLevel ApplicationRequired
```

**Lookup Attribute Features:**
- Creates both the lookup attribute and OneToMany relationship in a single operation
- **RelationshipSchemaName**: Optional parameter to specify a custom relationship name. If not provided, auto-generates as `{target}_{referencing}_{attribute}`
- **Cascade behaviors**: Configure Assign, Share, Unshare, Reparent, Delete, and Merge operations
- **Single-target only**: Multi-target (polymorphic) lookups not yet supported - use `Set-DataverseRelationshipMetadata` for complex scenarios
- **Update support**: Can update display name, description, and required level of existing lookups
- **Immutable properties**: Target entities, relationship name, and cascade behaviors cannot be changed after creation

### Updating StatusCode Options

For custom entities, you can update the statuscode attribute to define custom status values. Each status value must be associated with a state (0 = Active, 1 = Inactive).

```powershell
# Define status options with State property
$statusOptions = @(
    @{Value=1; Label='Draft'; State=0},
    @{Value=807290000; Label='Approved'; State=0},
    @{Value=807290001; Label='Filled'; State=0},
    @{Value=2; Label='Closed'; State=1},
    @{Value=807290002; Label='Cancelled'; State=1}
)

# Update statuscode options
Set-DataverseAttributeMetadata `
    -EntityName abc_position `
    -AttributeName statuscode `
    -Options $statusOptions
```

**StatusCode Options Requirements:**
- **State property is required**: Each option must include a State value (0 for Active state, 1 for Inactive state)
- **Value property is required**: Specify the numeric value for each status
- **Label property is required**: Specify the display name for each status
- **Custom values**: Use values 807290000 and higher to avoid conflicts with default values
- **Cannot modify system entities**: This only works for custom entities (with new_ prefix)

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

### Alternate Keys

Alternate keys allow records to be uniquely identified using a combination of attributes instead of the primary GUID key. They're particularly useful for data integration scenarios.

```powershell
# Create a single-attribute alternate key
Set-DataverseEntityKeyMetadata `
    -EntityName contact `
    -SchemaName contact_email_key `
    -KeyAttributes @('emailaddress1') `
    -DisplayName "Email Key" `
    -Publish

# Create a composite alternate key (multiple attributes)
Set-DataverseEntityKeyMetadata `
    -EntityName contact `
    -SchemaName contact_name_key `
    -KeyAttributes @('firstname', 'lastname') `
    -DisplayName "Name Key" `
    -Publish

# Retrieve all keys for an entity
$keys = Get-DataverseEntityKeyMetadata -EntityName contact

# Retrieve a specific key
$emailKey = Get-DataverseEntityKeyMetadata -EntityName contact -KeyName contact_email_key

# Access key properties
$emailKey.LogicalName          # contact_email_key
$emailKey.KeyAttributes         # Array of attribute names
$emailKey.DisplayName.UserLocalizedLabel.Label

# Delete an alternate key
Remove-DataverseEntityKeyMetadata `
    -EntityName contact `
    -KeyName contact_email_key `
    -Confirm:$false
```

**Important Notes:**
- Keys must be published before they become active
- Use `-Publish` parameter or manually publish customizations
- Alternate keys **cannot be updated** after creation - you must remove and recreate them
- Use `-Force` parameter to skip existence checks when creating keys
- Keys enable efficient upsert operations using alternate key values instead of GUIDs

## Related Resources

<!-- Link to Metadata CRUD examples removed. See the docs for comprehensive examples of all attribute types and scenarios -->
- [Microsoft SDK Documentation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.metadata) - SDK metadata object reference
- [Dataverse Metadata Reference](https://docs.microsoft.com/en-us/power-apps/developer/data-platform/metadata-services) - Overview of metadata concepts
