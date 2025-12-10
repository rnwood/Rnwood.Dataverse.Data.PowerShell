---
title: "Get-DataverseOptionSetMetadata - Get status reason options"
tags: ['Metadata']
source: "Get-DataverseOptionSetMetadata.md"
---
Retrieves status reason options with their colors and state values.

```powershell
$statusOptions = Get-DataverseOptionSetMetadata -EntityName opportunity -AttributeName statuscode
$statusOptions.Options | Select-Object Value, 
    @{N='Label';E={$_.Label.UserLocalizedLabel.Label}},
    @{N='State';E={$_.State}},
    @{N='Color';E={$_.Color}}

# Value Label            State Color
# ----- -----           ----- -----
# 1     Open             0     #0078D4
# 2     Won              1     #107C10
# 3     Lost             2     #D13438

```
