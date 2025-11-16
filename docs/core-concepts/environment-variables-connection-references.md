
# Environment Variables and Connection References

This guide explains how to work with environment variables and connection references in Dataverse using PowerShell.

## Overview

**Environment Variables** provide a way to store configuration values that can vary between environments (development, test, production) without modifying your solution. They are key-value pairs that can be referenced by Power Apps, Power Automate flows, and other Dataverse components.

**Connection References** allow Power Platform components (especially Power Automate flows) to use connections without hard-coding connection details. This makes solutions portable across environments.

## Environment Variables

### Understanding Environment Variables

Environment variables in Dataverse consist of two entities:
- **Environment Variable Definition** (`environmentvariabledefinition`) - Defines the variable with its schema name, display name, type, and optional default value
- **Environment Variable Value** (`environmentvariablevalue`) - Stores the actual value for the environment

This separation allows solutions to carry the definition while each environment provides its own value.

### Working with Environment Variable Definitions

Use definition cmdlets when you need to manage the structure and metadata of environment variables:

```powershell
# Create a new environment variable definition
Set-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl" `
    -DisplayName "API URL" `
    -Description "URL for the external API" `
    -Type "String"  # String type

# Query environment variable definitions
Get-DataverseEnvironmentVariableDefinition -SchemaName "new_api*"

# Update an existing definition
Set-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl" `
    -Description "Updated description for the API URL"

# Remove an environment variable definition
Remove-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl"
```

### Working with Environment Variable Values

Use value cmdlets when definitions are managed by solutions and you only need to set environment-specific values:

```powershell
# Set a value (definition must already exist)
Set-DataverseEnvironmentVariableValue -SchemaName "new_apiurl" `
    -Value "https://api.staging.example.com"

# Set multiple values at once
Set-DataverseEnvironmentVariableValue -EnvironmentVariableValues @{
    'new_apiurl' = 'https://api.production.example.com'
    'new_apikey' = 'prod-key-12345'
    'new_timeout' = '30'
}

# Query only values
Get-DataverseEnvironmentVariableValue -SchemaName "new_apiurl"

# Remove value but keep definition
Remove-DataverseEnvironmentVariableValue -SchemaName "new_apiurl"
```

### When to Use Which Cmdlet Set

**Use Definition Cmdlets** when:
- Creating new environment variable definitions from scratch
- You need to set or update display names, descriptions, or types
- Managing the structure of environment variables
- You want to query definition metadata only

**Use Value Cmdlets** when:
- Environment variable definitions already exist (typically managed by solutions)
- You only need to set or update the actual values
- Deploying solutions across multiple environments with different values
- Automating environment-specific configuration

### Common Workflows

#### Deploying a Solution with Environment Variables

```powershell
# Import solution (definitions included)
Import-DataverseSolution -InFile "MySolution.zip"

# Set environment-specific values
Set-DataverseEnvironmentVariableValue -EnvironmentVariableValues @{
    'new_apiurl' = 'https://api.production.example.com'
    'new_dbconnection' = 'Server=prod-sql;Database=ProdDB'
    'new_batchsize' = '100'
}
```

#### Creating Environment Variables Without Solutions

```powershell
# Create environment variable definitions
Set-DataverseEnvironmentVariableDefinition -SchemaName "new_emailtemplate" `
    -DisplayName "Email Template Name" `
    -Description "Name of the email template to use for welcome messages" `
    -Type "String"  # String type

Set-DataverseEnvironmentVariableDefinition -SchemaName "new_retrycount" `
    -DisplayName "Retry Count" `
    -Description "Number of times to retry failed operations" `
    -Type "Number"  # Integer type

# Set the actual values separately
Set-DataverseEnvironmentVariableValue -SchemaName "new_emailtemplate" -Value "Welcome"
Set-DataverseEnvironmentVariableValue -SchemaName "new_retrycount" -Value "3"
```

#### Migrating Values Between Environments

```powershell
# Export from source environment
$sourceConnection = Get-DataverseConnection -Url "https://source.crm.dynamics.com" -Interactive
$values = Get-DataverseEnvironmentVariableValue -Connection $sourceConnection

# Transform and import to target environment
$targetConnection = Get-DataverseConnection -Url "https://target.crm.dynamics.com" -Interactive
$valuesToSet = @{}
foreach ($v in $values) {
    # Apply environment-specific transformations if needed
    $valuesToSet[$v.SchemaName] = $v.Value.Replace("source", "target")
}
Set-DataverseEnvironmentVariableValue -Connection $targetConnection -EnvironmentVariableValues $valuesToSet
```

## Connection References

### Understanding Connection References

Connection References (`connectionreference`) provide a layer of abstraction between Power Platform components and actual connections. Instead of components referencing a specific connection ID, they reference a connection reference by its logical name. Each environment then maps that reference to an appropriate connection.

### Working with Connection References

```powershell
# Query connection references
Get-DataverseConnectionReference

# Query specific connection reference
Get-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharepoint"

# Query connection references by display name (supports wildcards)
Get-DataverseConnectionReference -DisplayName "Production*"

# Query connection references by connector ID
Get-DataverseConnectionReference -ConnectorId "98765432-4321-4321-4321-210987654321"

# Create a new connection reference
Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharepoint" `
    -ConnectionId "12345678-1234-1234-1234-123456789012" `
    -ConnectorId "98765432-4321-4321-4321-210987654321" `
    -DisplayName "Production SharePoint Site" `
    -Description "Connection to the main SharePoint document library"

# Update an existing connection reference
Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharepoint" `
    -ConnectionId "87654321-4321-4321-4321-210987654321"

# Set multiple connection references at once (update only - references must already exist)
Set-DataverseConnectionReference -ConnectionReferences @{
    'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
    'new_sql' = '87654321-4321-4321-4321-210987654321'
}

# Remove a connection reference
Remove-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharepoint"
```

**Note**: The single parameter set (`-ConnectionReferenceLogicalName`) can create new connection references if they don't exist, while the multiple parameter set (`-ConnectionReferences`) only updates existing ones.

### Finding Connection IDs

Connection references need valid connection IDs. To find available connections:

```powershell
# Query connections by name
$conn = Get-DataverseRecord -TableName connection -Filter "name eq 'Production SharePoint'"
Write-Host "Connection ID: $($conn.connectionid)"

# List all connections
Get-DataverseRecord -TableName connection -Columns connectionid, name, connectorid | 
    Format-Table -AutoSize
```

### Common Workflows

#### Deploying a Solution with Connection References

```powershell
# Import solution (connection references included)
Import-DataverseSolution -InFile "MySolution.zip"

# Set connection references to environment-specific connections
Set-DataverseConnectionReference -ConnectionReferences @{
    'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
    'new_sql' = '87654321-4321-4321-4321-210987654321'
    'new_customapi' = '11111111-2222-3333-4444-555555555555'
}
```

#### Setting Connection References During Solution Import

```powershell
# Set both environment variables and connection references during import
Import-DataverseSolution -InFile "MySolution.zip" `
    -EnvironmentVariables @{
        'new_apiurl' = 'https://api.production.example.com'
        'new_apikey' = 'prod-key-12345'
    } `
    -ConnectionReferences @{
        'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
        'new_sql' = '87654321-4321-4321-4321-210987654321'
    }
```

#### Creating Connection References Manually

If you need to create connection references outside of solution deployment, use the single parameter set:

```powershell
# Create connection references with full metadata
Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_azureblob" `
    -ConnectionId "11111111-2222-3333-4444-555555555555" `
    -ConnectorId "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee" `
    -DisplayName "Azure Blob Storage" `
    -Description "Connection to Azure Blob Storage for file uploads"

Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sendgrid" `
    -ConnectionId "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee" `
    -ConnectorId "ffffffff-gggg-hhhh-iiii-jjjjjjjjjjjj" `
    -DisplayName "SendGrid Email Service" `
    -Description "SMTP connection for sending transactional emails"
```

## Best Practices

### Environment Variables

1. **Use Descriptive Names**: Schema names should clearly indicate the purpose (e.g., `new_apiurl`, `new_maxretries`)
2. **Set Display Names**: Always provide display names for better user experience in the maker portal
3. **Use Default Values**: Set sensible defaults in the definition for development environments
4. **Document with Descriptions**: Use the description field to explain the variable's purpose and expected format
5. **Separate by Environment Type**: Use value cmdlets when deploying to different environment tiers

### Connection References

1. **Name by Function**: Use logical names that describe what the connection is for (e.g., `new_sharepoint_documents`, `new_sql_inventory`)
2. **Validate Before Import**: Ensure connections exist in the target environment before importing solutions
3. **Document Requirements**: Maintain a list of required connection references and their connector types
4. **Test Connections**: After setting references, test the flows/apps to ensure connections work correctly
5. **Use Service Accounts**: For production, use service accounts rather than user-specific connections
6. **Create vs Update**: Use the single parameter set to create new references (requires ConnectorId), use the multiple parameter set for bulk updates of existing references

### Automation Scripts

1. **Use Configuration Files**: Store environment-specific values in JSON or CSV files
2. **Implement Error Handling**: Check for missing definitions before setting values
3. **Log Changes**: Keep audit trail of configuration changes across environments
4. **Use WhatIf**: Test deployment scripts with -WhatIf before running for real
5. **Version Control**: Keep configuration files in source control alongside solution files

## See Also

- [Solution Management](../advanced/solution-management.md) - Importing and exporting solutions
- [Set-DataverseEnvironmentVariableDefinition](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseEnvironmentVariableDefinition.md)
- [Set-DataverseEnvironmentVariableValue](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseEnvironmentVariableValue.md)
- [Set-DataverseConnectionReference](../../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseConnectionReference.md)
- [Import-DataverseSolution](../../Rnwood.Dataverse.Data.PowerShell/docs/Import-DataverseSolution.md)
