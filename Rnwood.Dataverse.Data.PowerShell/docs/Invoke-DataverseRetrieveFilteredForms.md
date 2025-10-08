---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveFilteredForms

## SYNOPSIS
Contains the data that is needed to retrieve the entity forms that are available for a specified user.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFilteredFormsRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveFilteredFormsRequest)

## SYNTAX

```
Invoke-DataverseRetrieveFilteredForms -Connection <ServiceClient> -EntityLogicalName <String> -FormType <OptionSetValue> -SystemUserId <Guid>
```

## DESCRIPTION
Contains the data that is needed to retrieve the entity forms that are available for a specified user.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveFilteredForms -Connection <ServiceClient> -EntityLogicalName <String> -FormType <OptionSetValue> -SystemUserId <Guid>
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

### -EntityLogicalName
Gets the name of the entity the attribute belongs to.

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

### -FormType
Gets or sets the type of form. Required.

```yaml
Type: OptionSetValue
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SystemUserId
Gets or sets the ID of the user. Required.

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

### Microsoft.Crm.Sdk.Messages.RetrieveFilteredFormsResponse
[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveFilteredFormsResponse](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveFilteredFormsResponse)
## NOTES

## RELATED LINKS
