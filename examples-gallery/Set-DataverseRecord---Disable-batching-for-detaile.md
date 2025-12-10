---
title: "Set-DataverseRecord - Disable batching for detailed error handling"
tags: ['Data']
source: "Set-DataverseRecord.md"
---
Disables batching by setting BatchSize to 1. Each record is sent in a separate request, which stops immediately on first error rather than continuing through the batch.

```powershell
$records | Set-DataverseRecord -TableName contact -BatchSize 1

```

