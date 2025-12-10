---
title: "Remove-DataverseFormSection - Remove sections based on conditions"
tags: ['Metadata']
source: "Remove-DataverseFormSection.md"
---
Removes sections that match specific criteria (legacy sections with no controls).

```powershell
$form = Get-DataverseForm -Entity 'account' -Name 'Information' -ParseFormXml
$sectionsToRemove = $form.ParsedForm.Tabs | Where-Object { $_.Name -eq 'Details' } | 
    ForEach-Object { $_.Sections } | Where-Object { 
    $_.Name -like 'Legacy*' -and $_.Controls.Count -eq 0 
}

foreach ($section in $sectionsToRemove) {
    Write-Host "Removing empty legacy section: $($section.Name)"
    Remove-DataverseFormSection -FormId $form.Id -TabName 'Details' -SectionName $section.Name
}

```

