---
title: "Remove-DataverseRelationshipMetadata - Delete with error handling"
tags: ['Metadata']
source: "Remove-DataverseRelationshipMetadata.md"
---
Deletes a relationship with proper error handling and publishes customizations.

```powershell
try {
    Remove-DataverseRelationshipMetadata -SchemaName new_obsolete_rel `
       -Force -ErrorAction Stop
    Write-Host "Relationship deleted successfully"
    
    # Publish customizations after deletion
    Invoke-DataversePublishAllXml
    Write-Host "Customizations published"
} catch {
    Write-Error "Failed to delete relationship: $($_.Exception.Message)"
}

```
