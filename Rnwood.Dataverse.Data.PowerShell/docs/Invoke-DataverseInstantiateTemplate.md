---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseInstantiateTemplate

## SYNOPSIS
Contains the parameters that are needed to create an email message from a template (email template).

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InstantiateTemplateRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.InstantiateTemplateRequest)

## SYNTAX

```
Invoke-DataverseInstantiateTemplate -Connection <ServiceClient> -TemplateId <Guid> -ObjectType <String> -ObjectId <Guid>
```

## DESCRIPTION
Contains the parameters that are needed to create an email message from a template (email template).

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseInstantiateTemplate -Connection <ServiceClient> -TemplateId <Guid> -ObjectType <String> -ObjectId <Guid>
```

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TemplateId
Gets or sets the ID of the template. Required.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ObjectType
Gets or sets the type of entity that is represented by the property. Required.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ObjectId
Gets or sets the ID of the record that the email is regarding. Required.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

Supports -WhatIf and -Confirm: This cmdlet supports PowerShell -WhatIf and -Confirm via SupportsShouldProcess. Use -WhatIf to preview actions without making changes.

## INPUTS

### None
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.InstantiateTemplateResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.InstantiateTemplateResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.InstantiateTemplateResponse)
## NOTES

## RELATED LINKS
