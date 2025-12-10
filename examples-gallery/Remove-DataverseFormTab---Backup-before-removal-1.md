---
title: "Remove-DataverseFormTab - Backup before removal"
tags: ['Metadata']
source: "Remove-DataverseFormTab.md"
---
Creates a backup of the form before removing the tab for safety.

```powershell
# First, backup the form XML
$form = Get-DataverseForm -Entity 'contact' -Name 'Information' -IncludeFormXml
$backup = @{
    FormId = $form.Id
    FormXml = $form.FormXml
    BackupDate = Get-Date
}
$backup | Export-Clixml -Path "FormBackup_$($form.Id)_$(Get-Date -Format 'yyyyMMdd_HHmmss').xml"

# Now safely remove the tab
Remove-DataverseFormTab -FormId $form.Id -TabName 'OldTab'
Write-Host "Tab removed. Backup saved for recovery if needed."

```

