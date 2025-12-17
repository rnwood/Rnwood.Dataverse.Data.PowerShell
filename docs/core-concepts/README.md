
# Core Concepts


This section covers the core concepts and operations for working with Dataverse data using PowerShell.

## Contents

- **[Connection Management](connections.md)** - Default connections, named connections, and connection persistence
- **[Querying Records](querying.md)** - Filtering, paging, sorting, linking tables, SQL queries, and using SDK requests
- **[Creating and Updating Records](creating-updating.md)** - Create, update, upsert operations with type conversion and batching
- **[Deleting Records](deleting.md)** - Delete operations with confirmation, batching, and SQL alternatives
- **[File Attachments](file-attachments.md)** - Upload, download, and delete file data from file columns
- **[Managing Web Resources](web-resources.md)** - Upload, download, and manage web resources (JavaScript, CSS, HTML, images, etc.) with file system integration
- **[Working with Metadata](metadata.md)** - Reading and managing schema (entities, attributes, relationships, option sets)
- **[Organization Settings](organization-settings.md)** - Getting and updating organization table columns and OrgDbOrgSettings XML
- **[Form Management](form-management.md)** - Managing forms, tabs, sections, and controls with positioning and FormXml manipulation
- **[Solution Component Management](solution-component-management.md)** - Managing individual solution components (entities, attributes, forms, views) within solutions
- **[Dependency Management](dependency-management.md)** - Understanding and managing component dependencies, checking dependencies before deletion, and validating solutions
- **[Error Handling and Batch Operations](error-handling.md)** - Error handling, batch processing, and retry logic
- **[Environment Variables and Connection References](environment-variables-connection-references.md)** - Managing configuration values and connection references across environments

## Key Topics

### Working with Records
- Querying with filters, paging, and sorting
- Column output and type conversions
- Creating, updating, and upserting records
- Deleting records safely
- Batch operations for efficiency
- Managing web resources (files, scripts, styles)
- Working with metadata (schema management)
- Managing forms, tabs, sections, and controls
- Managing solution components within solutions
- Understanding and managing component dependencies

### Connection Features
- Default connections for simplified scripting
- Named connections for environment management
- Secure credential storage
- Multiple authentication methods

### Performance and Reliability
- Automatic paging for large result sets
- Batching for bulk operations
- Retry logic for transient failures
- Error handling best practices

## See Also

- [Getting Started](../getting-started/) - Installation and quick start guides
- [Advanced Topics](../advanced/) - Parallelization and solution management
- [Cmdlet Reference](../../Rnwood.Dataverse.Data.PowerShell/docs/) - Full cmdlet documentation
