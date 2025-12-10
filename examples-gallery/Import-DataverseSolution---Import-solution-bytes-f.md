---
title: "Import-DataverseSolution - Import solution bytes from pipeline"
tags: ['Solutions']
source: "Import-DataverseSolution.md"
---
Imports solution from a byte array via pipeline.

```powershell
$bytes = [System.IO.File]::ReadAllBytes("C:\Solutions\MySolution.zip")
$bytes | Import-DataverseSolution -OverwriteUnmanagedCustomizations

```
