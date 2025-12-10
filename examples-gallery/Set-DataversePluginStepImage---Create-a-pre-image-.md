---
title: "Set-DataversePluginStepImage - Create a pre-image with specific attributes"
tags: ['Plugins']
source: "Set-DataversePluginStepImage.md"
---
Creates a pre-image with specific attributes.

```powershell
Set-DataversePluginStepImage `
   -SdkMessageProcessingStepId $stepId `
   -EntityAlias "PreImage" `
   -ImageType 0 `
   -Attributes "firstname","lastname","emailaddress1"

```

