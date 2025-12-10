---
title: "Set-DataverseEntityMetadata - Create entity with audit and change tracking enabled"
tags: ['Metadata']
source: "Set-DataverseEntityMetadata.md"
---
Creates an entity with auditing and change tracking enabled for compliance requirements.

```powershell
Set-DataverseEntityMetadata -EntityName new_transaction `
   -SchemaName new_Transaction `
   -DisplayName "Transaction" `
   -DisplayCollectionName "Transactions" `
   -OwnershipType UserOwned `
   -IsAuditEnabled `
   -ChangeTrackingEnabled `
   -PrimaryAttributeSchemaName new_name `
   -PrimaryAttributeDisplayName "Transaction ID"

```
