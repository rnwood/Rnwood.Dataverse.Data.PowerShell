---
title: "Set-DataverseForm - Create a form with custom FormXml"
tags: ['Metadata']
source: "Set-DataverseForm.md"
---
Creates a new form using custom FormXml content and publishes it immediately.

```powershell
$formXml = Get-Content -Path 'CustomForm.xml' -Raw
Set-DataverseForm -Entity 'contact' -Name 'Advanced Form' -FormType 'Main' -FormXmlContent $formXml -Publish

```

