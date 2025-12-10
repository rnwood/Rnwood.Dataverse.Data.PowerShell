---
title: "Get-DataverseSolutionFile - Parse a solution file from bytes in pipeline"
tags: ['Solutions']
source: "Get-DataverseSolutionFile.md"
---
Reads solution bytes and pipes them to the cmdlet for parsing.

```powershell
$bytes = [System.IO.File]::ReadAllBytes("C:\Solutions\MySolution.zip")
$bytes | Get-DataverseSolutionFile

# UniqueName            : MySolution
# Name                  : My Solution
# Version               : 1.0.0.0
# IsManaged             : False

```
