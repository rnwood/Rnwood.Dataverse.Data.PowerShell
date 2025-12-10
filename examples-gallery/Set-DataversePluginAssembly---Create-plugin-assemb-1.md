---
title: "Set-DataversePluginAssembly - Create plugin assembly with PassThru"
tags: ['Plugins']
source: "Set-DataversePluginAssembly.md"
---
Creates a new plugin assembly and returns the created object.

```powershell
$assembly = Set-DataversePluginAssembly -Name "MyPlugin" -FilePath "C:\Plugins\MyPlugin.dll" -PassThru
$assembly.Id

```

