---
title: "Remove-DataverseAttributeMetadata - Delete with error handling"
tags: ['Metadata']
source: "Remove-DataverseAttributeMetadata.md"
---
Deletes an attribute with proper error handling and publishes customizations afterward.

```powershell
try {
    Remove-DataverseAttributeMetadata -EntityName account -AttributeName new_obsolete `
       -Force -ErrorAction Stop
    Write-Host "Attribute deleted successfully"
    
    # Publish customizations after deletion
    Invoke-DataversePublishAllXml
    Write-Host "Customizations published"
} catch {
    Write-Error "Failed to delete attribute: $($_.Exception.Message)"
}

```
