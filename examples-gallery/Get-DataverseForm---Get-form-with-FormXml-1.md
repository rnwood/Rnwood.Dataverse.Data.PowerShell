---
title: "Get-DataverseForm - Get form with FormXml"
tags: ['Metadata']
source: "Get-DataverseForm.md"
---
Retrieves a form and includes the raw FormXml content.

```powershell
$form = Get-DataverseForm -Entity 'contact' -Name 'Information' -IncludeFormXml
$form.FormXml

```

