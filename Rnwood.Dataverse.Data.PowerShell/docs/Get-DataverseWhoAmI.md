---
external help file: Rnwood.Dataverse.Data.PowerShell.FrameworkSpecific.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseWhoAmI

## SYNOPSIS
Retrieves details about the current Dataverse user and organization specified by the connection provided.

## SYNTAX

```
Get-DataverseWhoAmI -Connection <ServiceClient> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataverseWhoAmI -Connection $c
```

Returns info for the existing connection `$c`.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnnection cmdlet

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

### -ProgressAction
See standard PS docs.

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

### Microsoft.Crm.Sdk.Messages.WhoAmIResponse

## NOTES
See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.whoamiresponse?view=dataverse-sdk-latest

## RELATED LINKS
