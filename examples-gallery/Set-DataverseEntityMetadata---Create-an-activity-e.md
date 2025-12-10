---
title: "Set-DataverseEntityMetadata - Create an activity entity"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates a custom activity entity that derives from activitypointer. Activity entities are used to track interactions like appointments, emails, phone calls, and custom activity types. Activity entities automatically support certain features like regarding objects and activity parties.

**Important Notes:**
- Activity entities derive from the activitypointer entity and inherit standard activity fields
- The `-IsActivity` switch can only be set during entity creation and cannot be changed afterwards
- Activity entities automatically appear in timeline controls and activity-related views
- The primary attribute is typically used as the "Subject" field for the activity

```powershell
Set-DataverseEntityMetadata -EntityName new_customactivity `
   -SchemaName new_CustomActivity `
   -DisplayName "Custom Activity" `
   -DisplayCollectionName "Custom Activities" `
   -Description "A custom activity entity for tracking interactions" `
   -OwnershipType UserOwned `
   -IsActivity `
   -PrimaryAttributeSchemaName new_subject `
   -PrimaryAttributeDisplayName "Subject" `
   -PrimaryAttributeMaxLength 200

```
