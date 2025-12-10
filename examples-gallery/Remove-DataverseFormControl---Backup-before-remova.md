---
title: "Remove-DataverseFormControl - Backup before removal"
tags: ['Metadata']
source: "Remove-DataverseFormControl.md"
---
Creates a backup of the form before removing the control for safety.

```powershell
# First, backup the form XML
$form = Get-DataverseForm -Entity 'contact' -Name 'Information' -IncludeFormXml
$backup = @{
    FormId = $form.Id
    FormXml = $form.FormXml
    BackupDate = Get-Date
}
$backup | Export-Clixml -Path "FormBackup_$($form.Id)_$(Get-Date -Format 'yyyyMMdd_HHmmss').xml"

# Now safely remove the control
Remove-DataverseFormControl -FormId $form.Id -TabName 'General' -SectionName 'ContactInfo' -DataField 'assistantname'
Write-Host "Control removed. Backup saved for recovery if needed."

```

