
# Rnwood.Dataverse.Data.PowerShell Documentation


Complete documentation for the Rnwood.Dataverse.Data.PowerShell module - a PowerShell module for connecting to Microsoft Dataverse and manipulating data.

## Documentation Sections

### [Getting Started](getting-started/)
Essential guides for new users:
- [Installation](getting-started/installation.md) - How to install and update the module
- [Quick Start Guide](getting-started/quickstart.md) - Basic operations and first steps
- [Authentication Methods](getting-started/authentication.md) - All supported authentication methods

### [Core Concepts](core-concepts/)
Fundamental operations and concepts:
- [Connection Management](core-concepts/connections.md) - Default and named connections
- [Querying Records](core-concepts/querying.md) - Filtering, paging, sorting, and SQL queries
- [Creating and Updating Records](core-concepts/creating-updating.md) - Create, update, upsert operations
- [Deleting Records](core-concepts/deleting.md) - Delete operations with safety features
- [Error Handling and Batch Operations](core-concepts/error-handling.md) - Error handling and retry logic
- [Environment Variables and Connection References](core-concepts/environment-variables-connection-references.md) - Managing configuration and connections

### [Advanced Topics](advanced/)
Advanced features for power users:
- [Parallelization](advanced/parallelization.md) - Parallel processing for best performance
- [Solution Management](advanced/solution-management.md) - Import, export, and manage solutions

### [Cmdlet Reference](../Rnwood.Dataverse.Data.PowerShell/docs/)
Complete reference documentation for all 390+ cmdlets.

## Quick Reference

### Main Cmdlets
- [`Get-DataverseConnection`](../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) - Create or retrieve a connection
- [`Get-DataverseRecord`](../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) - Query and retrieve records
- [`Set-DataverseRecord`](../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseRecord.md) - Create, update or upsert records
- [`Remove-DataverseRecord`](../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseRecord.md) - Delete records
- [`Invoke-DataverseRequest`](../Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseRequest.md) - Execute arbitrary SDK requests
- [`Invoke-DataverseSql`](../Rnwood.Dataverse.Data.PowerShell/docs/Invoke-DataverseSql.md) - Run SQL queries

### Environment Variables
- [`Get-DataverseEnvironmentVariableDefinition`](../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseEnvironmentVariableDefinition.md) - Query definitions with values
- [`Set-DataverseEnvironmentVariableDefinition`](../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseEnvironmentVariableDefinition.md) - Create or update definitions and values
- [`Remove-DataverseEnvironmentVariableDefinition`](../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseEnvironmentVariableDefinition.md) - Remove definitions
- [`Get-DataverseEnvironmentVariableValue`](../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseEnvironmentVariableValue.md) - Query values
- [`Set-DataverseEnvironmentVariableValue`](../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseEnvironmentVariableValue.md) - Set values
- [`Remove-DataverseEnvironmentVariableValue`](../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseEnvironmentVariableValue.md) - Remove values

### Connection References
- [`Get-DataverseConnectionReference`](../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnectionReference.md) - Query connection references
- [`Set-DataverseConnectionReference`](../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseConnectionReference.md) - Update connection reference values
- [`Remove-DataverseConnectionReference`](../Rnwood.Dataverse.Data.PowerShell/docs/Remove-DataverseConnectionReference.md) - Remove connection references

### Common Tasks
- **First time setup**: [Installation Guide](getting-started/installation.md)
- **Connect to Dataverse**: [Quick Start](getting-started/quickstart.md#getting-a-connection)
- **Query data**: [Querying Records](core-concepts/querying.md)
- **Create/update data**: [Creating and Updating](core-concepts/creating-updating.md)
- **Bulk operations**: [Parallelization](advanced/parallelization.md)
- **Manage solutions**: [Solution Management](advanced/solution-management.md)

## Additional Resources

- [Main README](../README.md) - Project overview and features
- [GitHub Repository](https://github.com/rnwood/Rnwood.Dataverse.Data.PowerShell) - Source code and issues
- [PowerShell Gallery](https://www.powershellgallery.com/packages/Rnwood.Dataverse.Data.PowerShell) - Published module
