---
title: "Set-DataverseView - Create view with complex filters"
tags: ['Metadata']
source: "Set-DataverseView.md"
---
Creates a system view with nested logical filter expressions using AND/OR operators.

```powershell
Set-DataverseView -PassThru `
   -Name "High Value Opportunities" `
   -TableName opportunity `
   -ViewType "System" `
   -Columns @("name", "estimatedvalue", "closeprobability", "actualclosedate") `
   -FilterValues @{
        and = @(
     @{ statecode = 0 },
      @{ or = @(
      @{ estimatedvalue = @{ value = 100000; operator = 'GreaterThan' } },
 @{ closeprobability = @{ value = 80; operator = 'GreaterThan' } }
    )}
        )
  }

```

