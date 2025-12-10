---
title: "Set-DataverseForm - Update form with new FormXml and publish"
tags: ['Metadata']
source: "Set-DataverseForm.md"
---
Updates an existing form with new FormXml content and publishes the changes.

```powershell
$newFormXml = Get-Content -Path 'UpdatedForm.xml' -Raw
Set-DataverseForm -Id $formId -FormXmlContent $newFormXml -Publish

```

