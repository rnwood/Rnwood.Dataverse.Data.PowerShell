---
title: "Get-DataverseEntityMetadata - Work with default connection"
tags: ['Metadata']
source: "Get-DataverseEntityMetadata.md"
---
Demonstrates using the default connection for simplified commands.

```powershell
# Set a default connection
$conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
Set-DataverseConnectionAsDefault onn

# Now get metadata without specifying connection
$metadata = Get-DataverseEntityMetadata -EntityName account
$metadata.LogicalName
# account

```

