<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
## Table of Contents

- [Core Concepts](#core-concepts)
  - [Contents](#contents)
  - [Key Topics](#key-topics)
    - [Working with Records](#working-with-records)
    - [Connection Features](#connection-features)
    - [Performance and Reliability](#performance-and-reliability)
  - [See Also](#see-also)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

# Core Concepts

<!-- TOC -->
<!-- /TOC -->

This section covers the core concepts and operations for working with Dataverse data using PowerShell.

## Contents

- **[Connection Management](connections.md)** - Default connections, named connections, and connection persistence
- **[Querying Records](querying.md)** - Filtering, paging, sorting, linking tables, SQL queries, and specialized cmdlets
- **[Creating and Updating Records](creating-updating.md)** - Create, update, upsert operations with type conversion and batching
- **[Deleting Records](deleting.md)** - Delete operations with confirmation, batching, and SQL alternatives
- **[Error Handling and Batch Operations](error-handling.md)** - Error handling, batch processing, and retry logic
- **[Environment Variables and Connection References](environment-variables-connection-references.md)** - Managing configuration values and connection references across environments

## Key Topics

### Working with Records
- Querying with filters, paging, and sorting
- Column output and type conversions
- Creating, updating, and upserting records
- Deleting records safely
- Batch operations for efficiency

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
