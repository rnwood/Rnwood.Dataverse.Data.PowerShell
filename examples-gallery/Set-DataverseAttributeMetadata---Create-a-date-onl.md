---
title: "Set-DataverseAttributeMetadata - Create a date-only attribute"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a date-only attribute (no time component) that displays in the user's local timezone.

```powershell
Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_hiredate `
   -SchemaName new_HireDate -DisplayName "Hire Date" -AttributeType DateTime `
   -DateTimeFormat DateOnly -DateTimeBehavior UserLocal

```
