---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseRetrieveAadUserSetOfPrivilegesByNames

## SYNOPSIS
Executes a RetrieveAadUserSetOfPrivilegesByNamesRequest against the Dataverse organization service.

[Microsoft Learn: Microsoft.Crm.Sdk.Messages.RetrieveAadUserSetOfPrivilegesByNamesRequest](https://learn.microsoft.com/dotnet/api/Microsoft.Crm.Sdk.Messages.RetrieveAadUserSetOfPrivilegesByNamesRequest)

## SYNTAX

```
Invoke-DataverseRetrieveAadUserSetOfPrivilegesByNames -Connection <ServiceClient> [-DirectoryObjectId <Guid>]
 [-PrivilegeNames <String[]>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Executes a RetrieveAadUserSetOfPrivilegesByNamesRequest against the Dataverse organization service.

## EXAMPLES

### Example 1
```powershell
PS C:\> Invoke-DataverseRetrieveAadUserSetOfPrivilegesByNames -Connection <ServiceClient> -DirectoryObjectId <Guid> -PrivilegeNames <String[]>
```

## PARAMETERS

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

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

### -DirectoryObjectId
The ID of the Directory Object.

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

### -PrivilegeNames
Gets or sets the PrivilegeNames for the request.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
