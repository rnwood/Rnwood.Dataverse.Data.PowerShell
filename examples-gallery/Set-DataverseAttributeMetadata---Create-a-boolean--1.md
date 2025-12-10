---
title: "Set-DataverseAttributeMetadata - Create a boolean (Yes/No) attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a Yes/No attribute with custom labels and a default value of true.

```powershell
Set-DataverseAttributeMetadata -EntityName account -AttributeName new_ispremium `
   -SchemaName new_IsPremium -DisplayName "Is Premium" -AttributeType Boolean `
   -TrueLabel "Premium" -FalseLabel "Standard" -DefaultValue $true

```
