---
title: "Set-DataverseView - Insert columns before a specific column"
tags: ['Metadata']
source: "Set-DataverseView.md"
---
Inserts the job title column before the email address column in the layout.

```powershell
Set-DataverseView -Id $viewId `
   -ViewType "Personal" `
   -AddColumns @("jobtitle") `
   -InsertColumnsBefore "emailaddress1"

```

