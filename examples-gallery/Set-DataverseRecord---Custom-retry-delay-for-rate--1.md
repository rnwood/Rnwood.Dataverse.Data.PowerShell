---
title: "Set-DataverseRecord - Custom retry delay for rate limiting"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Processes a large dataset with automatic retry configured for rate limiting scenarios. Uses 5 retry attempts with an initial 2-second delay, doubling on each retry (2s, 4s, 8s, 16s, 32s).

```powershell
# Handle rate limiting with longer initial delay
$largeDataset | Set-DataverseRecord -TableName account -Retries 5 -InitialRetryDelay 2000

```

