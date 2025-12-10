---
title: "Get-DataverseEntityMetadata"
tags: ['CRUD', 'Metadata']
source: "Metadata-CRUD-Examples.md"
---
Lists all entities with filtering options.

```powershell
# List all entities (basic info)
Get-DataverseEntityMetadata

# List with detailed information
Get-DataverseEntityMetadata -IncludeDetails

# Filter to only custom entities
Get-DataverseEntityMetadata -OnlyCustom -IncludeDetails

# Filter to only managed entities
Get-DataverseEntityMetadata -OnlyManaged

# Filter to only customizable entities
Get-DataverseEntityMetadata -OnlyCustomizable

```
