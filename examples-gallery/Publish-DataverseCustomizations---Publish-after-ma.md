---
title: "Publish-DataverseCustomizations - Publish after making schema changes"
tags: ['Solutions']
source: "Publish-DataverseCustomizations.md"
---
Adds a new field and publishes the changes to make them available.

```powershell
# Add a new field to contact entity
Invoke-DataverseCreateAttribute -EntityName "contact" -AttributeName "new_customfield" -AttributeType "String"
# Publish the changes
Publish-DataverseCustomizations -EntityName "contact"

```
