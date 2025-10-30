<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Rnwood.Dataverse.Data.PowerShell Documentation](#rnwooddataversedatapowershell-documentation)
  - [Documentation Sections](#documentation-sections)
    - [Getting Started](#getting-started)
    - [Core Concepts](#core-concepts)
    - [Advanced Topics](#advanced-topics)
    - [Cmdlet Reference](#cmdlet-reference)
  - [Quick Reference](#quick-reference)
    - [Main Cmdlets](#main-cmdlets)
    - [Common Tasks](#common-tasks)
  - [Additional Resources](#additional-resources)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Rnwood.Dataverse.Data.PowerShell Documentation

<!-- TOC -->
<!-- /TOC -->

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
- [`Set-DataverseEnvironmentVariable`](../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseEnvironmentVariable.md) - Set environment variable values
- [`Set-DataverseConnectionReference`](../Rnwood.Dataverse.Data.PowerShell/docs/Set-DataverseConnectionReference.md) - Set connection reference values

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
