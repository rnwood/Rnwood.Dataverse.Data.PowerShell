---
title: "Remove-DataverseFormSection - Backup before removal"
tags: ['Metadata']
source: "Remove-DataverseFormSection.md"
---
Creates a backup of the form before removing the section for safety.

```powershell
# First, backup the form XML
$form = Get-DataverseForm -Entity 'contact' -Name 'Information' -IncludeFormXml
$backup = @{
    FormId = $form.Id
    FormXml = $form.FormXml
    BackupDate = Get-Date
}
$backup | Export-Clixml -Path "FormBackup_$($form.Id)_$(Get-Date -Format 'yyyyMMdd_HHmmss').xml"

# Now safely remove the section
Remove-DataverseFormSection -FormId $form.Id -TabName 'General' -SectionName 'OldSection'
Write-Host "Section removed. Backup saved for recovery if needed."

```

