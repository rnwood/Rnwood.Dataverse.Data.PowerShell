---
title: "Get-DataverseSolutionFile - Parse a solution file from disk"
tags: ['Solutions']
source: "Get-DataverseSolutionFile.md"
---
Parses the solution file and displays all metadata.

```powershell
Get-DataverseSolutionFile -Path "C:\Solutions\MySolution_1_0_0_0.zip"

# UniqueName            : MySolution
# Name                  : My Solution
# Version               : 1.0.0.0
# IsManaged             : False
# Description           : My custom solution for business processes
# PublisherName         : Contoso
# PublisherUniqueName   : contoso
# PublisherPrefix       : con

```
