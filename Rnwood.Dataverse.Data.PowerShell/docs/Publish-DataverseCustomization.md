---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Publish-DataverseCustomization

## SYNOPSIS
Publishes customizations in Dataverse.

## SYNTAX

```
Publish-DataverseCustomization -Connection <ServiceClient> [[-ParameterXml] <String>] 
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet publishes customizations in Dataverse using the PublishXmlRequest message.

Publishing makes changes to entities, forms, views, and other customizations available to users.

## EXAMPLES

### Example 1
```powershell
PS C:\> Publish-DataverseCustomization -Connection $c
```

Publishes all customizations.

### Example 2
```powershell
PS C:\> Publish-DataverseCustomization -Connection $c -ParameterXml "<importexportxml><entities><entity>account</entity></entities></importexportxml>"
```

Publishes customizations for a specific entity.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

### -ParameterXml
XML string specifying which customizations to publish. If not specified, publishes all customizations.

### -ProgressAction
See standard PS docs.

### CommonParameters
This cmdlet supports the common parameters.

## INPUTS

### None

## OUTPUTS

### Microsoft.Crm.Sdk.Messages.PublishXmlResponse

## NOTES

See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.publishxmlrequest?view=dataverse-sdk-latest

## RELATED LINKS
