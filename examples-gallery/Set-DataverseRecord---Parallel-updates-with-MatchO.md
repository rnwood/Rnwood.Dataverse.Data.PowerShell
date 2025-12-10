---
title: "Set-DataverseRecord - Parallel updates with MatchOn"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Updates 1000 existing contacts in parallel using email address as the match key. Uses 3 workers with batch size of 50 for optimal performance.

```powershell
# Update records in parallel by email address
$updates = 1..1000 | ForEach-Object {
    @{ 
        emailaddress1 = "user$_@example.com"
        telephone1 = "555-$_"
    }
}

$updates | Set-DataverseRecord -TableName contact -MatchOn emailaddress1 -MaxDegreeOfParallelism 3 -BatchSize 50

```

