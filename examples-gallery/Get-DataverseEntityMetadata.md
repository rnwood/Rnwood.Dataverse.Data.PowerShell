---
title: "Get-DataverseEntityMetadata"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Retrieves entity metadata with detailed information.

**Note:** By default, these cmdlets retrieve unpublished (draft) metadata which includes all changes. Use the `-Published` switch to retrieve only published metadata.

```powershell
# Get metadata for a specific entity
Get-DataverseEntityMetadata -EntityName contact

# Get only published metadata
Get-DataverseEntityMetadata -EntityName contact -Published

# Include attribute metadata
Get-DataverseEntityMetadata -EntityName contact -IncludeAttributes

# Include relationships
Get-DataverseEntityMetadata -EntityName contact -IncludeRelationships

# Include all metadata (attributes, relationships, privileges)
Get-DataverseEntityMetadata -EntityName contact -IncludeAttributes -IncludeRelationships -IncludePrivileges

# List all entities in the environment
Get-DataverseEntityMetadata

# List all entities with attributes included
Get-DataverseEntityMetadata -IncludeAttributes

```
