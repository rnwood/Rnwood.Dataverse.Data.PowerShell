---
title: "Set-DataverseView - Add columns at specific positions"
tags: ['Metadata']
source: "Set-DataverseView.md"
---
Adds mobile phone and fax columns after the telephone1 column.

```powershell
Set-DataverseView -Id $viewId `
   -ViewType "Personal" `
   -AddColumns @("mobilephone", "fax") `
   -InsertColumnsAfter "telephone1"

```

