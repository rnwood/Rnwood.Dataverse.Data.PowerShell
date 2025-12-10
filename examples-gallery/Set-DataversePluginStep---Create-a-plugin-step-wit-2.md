---
title: "Set-DataversePluginStep - Create a plugin step with filtering attributes"
tags: ['Plugins']
source: "Set-DataversePluginStep.md"
---
Creates a synchronous pre-operation plugin step with filtering attributes.

```powershell
Set-DataversePluginStep -Name "MyStep" -PluginTypeId $typeId `
   -SdkMessageId $messageId -Stage 20 -Mode 0 `
   -FilteringAttributes "name","revenue","primarycontactid"

```

