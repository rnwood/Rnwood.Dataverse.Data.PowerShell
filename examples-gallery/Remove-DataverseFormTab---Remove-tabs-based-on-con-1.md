---
title: "Remove-DataverseFormTab - Remove tabs based on conditions"
tags: ['Metadata']
source: "Remove-DataverseFormTab.md"
---
Removes tabs that match specific criteria (legacy tabs with no sections).

```powershell
$form = Get-DataverseForm -Entity 'account' -Name 'Information' -ParseFormXml
$tabsToRemove = $form.ParsedForm.Tabs | Where-Object { 
    $_.Name -like 'Legacy*' -and $_.Sections.Count -eq 0 
}

foreach ($tab in $tabsToRemove) {
    Write-Host "Removing empty legacy tab: $($tab.Name)"
    Remove-DataverseFormTab -FormId $form.Id -TabName $tab.Name
}

```

