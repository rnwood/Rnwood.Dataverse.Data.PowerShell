---
title: "Remove-DataverseFormControl - Remove obsolete controls based on conditions"
tags: ['Metadata']
source: "Remove-DataverseFormControl.md"
---
Removes controls that match specific criteria (hidden legacy controls).

```powershell
$form = Get-DataverseForm -Entity 'account' -Name 'Information' -ParseFormXml
$controlsToRemove = $form.ParsedForm.Tabs | 
    ForEach-Object { $_.Sections } |
    ForEach-Object { $_.Controls } | 
    Where-Object { $_.DataField -like 'legacy_*' -and $_.Hidden -eq $true }

foreach ($control in $controlsToRemove) {
    Write-Host "Removing hidden legacy control: $($control.DataField)"
    Remove-DataverseFormControl -FormId $form.Id -ControlId $control.Id
}

```

