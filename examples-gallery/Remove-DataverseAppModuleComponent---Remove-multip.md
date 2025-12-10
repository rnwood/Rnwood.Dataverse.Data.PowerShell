---
title: "Remove-DataverseAppModuleComponent - Remove multiple components safely"
tags: ['Metadata']
source: "Remove-DataverseAppModuleComponent.md"
---
Removes multiple components safely with error suppression.

```powershell
$componentsToRemove = @(
    @{ AppModuleUniqueName = "myapp"; ObjectId = $entity1Id },
    @{ AppModuleUniqueName = "myapp"; ObjectId = $entity2Id },
    @{ AppModuleUniqueName = "myapp"; ObjectId = $formId }
)
$componentsToRemove | ForEach-Object {
    Remove-DataverseAppModuleComponent -AppModuleUniqueName $_.AppModuleUniqueName -ObjectId $_.ObjectId -IfExists -Confirm:$false
}

```

