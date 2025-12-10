---
title: "Remove-DataverseSolutionComponent - Remove an entity component from a solution"
tags: ['Solutions']
source: "Remove-DataverseSolutionComponent.md"
---
Removes an entity component from the solution. The entity itself remains in the environment.

```powershell
$connection = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
$entityMetadata = Get-DataverseEntityMetadata -EntityName "new_customtable"
Remove-DataverseSolutionComponent -SolutionName "MySolution" `
   -ComponentId $entityMetadata.MetadataId -ComponentType 1

```

