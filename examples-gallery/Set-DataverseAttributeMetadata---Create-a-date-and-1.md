---
title: "Set-DataverseAttributeMetadata - Create a date and time attribute with timezone independence"
tags: ['Metadata']
source: "Set-DataverseAttributeMetadata.md"
---
Creates a date and time attribute that stores values independent of timezone.

```powershell
Set-DataverseAttributeMetadata -EntityName appointment -AttributeName new_eventtime `
   -SchemaName new_EventTime -DisplayName "Event Time" -AttributeType DateTime `
   -DateTimeFormat DateAndTime -DateTimeBehavior TimeZoneIndependent

```
