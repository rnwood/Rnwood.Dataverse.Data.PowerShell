---
title: "Set-DataversePluginStepImage - Create a post-image with all attributes"
tags: ['Plugins']
source: "Set-DataversePluginStepImage.md"
---
Creates a post-image with all attributes (no -Attributes parameter means all attributes are included).

```powershell
Set-DataversePluginStepImage `
   -SdkMessageProcessingStepId $stepId `
   -EntityAlias "PostImage" `
   -ImageType 1

```

