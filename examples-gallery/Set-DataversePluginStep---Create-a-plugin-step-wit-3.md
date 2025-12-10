---
title: "Set-DataversePluginStep - Create a plugin step without filtering"
tags: ['Plugins']
source: "Set-DataversePluginStep.md"
---
Creates a synchronous pre-operation plugin step without filtering attributes.

```powershell
Set-DataversePluginStep -Name "MyStep" -PluginTypeId $typeId `
   -SdkMessageId $messageId -Stage 20 -Mode 0

```

