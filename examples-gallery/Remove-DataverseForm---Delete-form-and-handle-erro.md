---
title: "Remove-DataverseForm - Delete form and handle errors gracefully"
tags: ['Metadata']
source: "Remove-DataverseForm.md"
---
Demonstrates error handling when deleting forms.

```powershell
try {
    Remove-DataverseForm -Entity 'contact' -Name 'NonexistentForm' -IfExists
    Write-Host "Form deletion completed successfully"
} catch {
    Write-Warning "Failed to delete form: $($_.Exception.Message)"
}

```

